using System.Text.Json;
using BalizasV16.Models;
using Microsoft.AspNetCore.SignalR;

namespace BalizasV16.Services;

public class EarthquakeService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHubContext<EarthquakeHub> _hubContext;
    private readonly ILogger<EarthquakeService> _logger;
    private readonly TimeSpan _pollInterval = TimeSpan.FromMinutes(5);

    // USGS feeds — free, no API key, GeoJSON format
    private const string UsgsAllDayUrl = "https://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/all_day.geojson";
    private const string UsgsAllWeekUrl = "https://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/all_week.geojson";
    private const string SsnRssUrl = "http://www.ssn.unam.mx/rss/ultimos-sismos.xml";

    public static List<Earthquake> CurrentEarthquakes { get; private set; } = new();
    public static DateTime LastUpdated { get; private set; } = DateTime.MinValue;

    public EarthquakeService(IHttpClientFactory httpClientFactory, IHubContext<EarthquakeHub> hubContext, ILogger<EarthquakeService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var earthquakes = await FetchFromUsgsAsync(stoppingToken);

                // Also try SSN Mexico for additional data
                try
                {
                    var ssnQuakes = await FetchFromSsnAsync(stoppingToken);
                    // Merge SSN data (deduplicate by proximity + time)
                    foreach (var ssn in ssnQuakes)
                    {
                        if (!earthquakes.Any(e =>
                            Math.Abs(e.Lat - ssn.Lat) < 0.1 &&
                            Math.Abs(e.Lon - ssn.Lon) < 0.1 &&
                            Math.Abs((e.Time - ssn.Time).TotalMinutes) < 10))
                        {
                            earthquakes.Add(ssn);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "SSN feed unavailable, using USGS only");
                }

                CurrentEarthquakes = earthquakes.OrderByDescending(e => e.Time).ToList();
                LastUpdated = DateTime.UtcNow;
                _logger.LogInformation("Fetched {Count} earthquakes ({Mexico} near Mexico)", earthquakes.Count,
                    earthquakes.Count(e => e.Lat >= 14 && e.Lat <= 33 && e.Lon >= -118 && e.Lon <= -86));

                await _hubContext.Clients.All.SendAsync("EarthquakesUpdated", CurrentEarthquakes, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching earthquake data");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }
    }

    private async Task<List<Earthquake>> FetchFromUsgsAsync(CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("USGS");
        var json = await client.GetStringAsync(UsgsAllWeekUrl, ct);

        using var doc = JsonDocument.Parse(json);
        var features = doc.RootElement.GetProperty("features");
        var earthquakes = new List<Earthquake>();

        foreach (var feature in features.EnumerateArray())
        {
            var props = feature.GetProperty("properties");
            var geom = feature.GetProperty("geometry");
            var coords = geom.GetProperty("coordinates");

            var mag = props.TryGetProperty("mag", out var m) && m.ValueKind == JsonValueKind.Number ? m.GetDouble() : 0;
            var timeMs = props.GetProperty("time").GetInt64();
            var time = DateTimeOffset.FromUnixTimeMilliseconds(timeMs).UtcDateTime;

            earthquakes.Add(new Earthquake
            {
                Id = feature.GetProperty("id").GetString() ?? "",
                Magnitude = mag,
                Place = props.TryGetProperty("place", out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() ?? "" : "",
                Time = time,
                Lon = coords[0].GetDouble(),
                Lat = coords[1].GetDouble(),
                Depth = coords[2].GetDouble(),
                Url = props.TryGetProperty("url", out var u) && u.ValueKind == JsonValueKind.String ? u.GetString() ?? "" : "",
                Felt = props.TryGetProperty("felt", out var f) && f.ValueKind == JsonValueKind.Number ? f.GetInt32() : null,
                Alert = props.TryGetProperty("alert", out var a) && a.ValueKind == JsonValueKind.String ? a.GetString() : null,
                Tsunami = props.TryGetProperty("tsunami", out var t) && t.ValueKind == JsonValueKind.Number ? t.GetInt32() : 0,
                MagType = props.TryGetProperty("magType", out var mt) && mt.ValueKind == JsonValueKind.String ? mt.GetString() ?? "" : "",
                Title = props.TryGetProperty("title", out var ti) && ti.ValueKind == JsonValueKind.String ? ti.GetString() ?? "" : "",
                Significance = props.TryGetProperty("sig", out var s) && s.ValueKind == JsonValueKind.Number ? s.GetInt32() : 0
            });
        }

        return earthquakes;
    }

    private async Task<List<Earthquake>> FetchFromSsnAsync(CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("SSN");
        var xml = await client.GetStringAsync(SsnRssUrl, ct);

        var doc = System.Xml.Linq.XDocument.Parse(xml);
        var earthquakes = new List<Earthquake>();
        var items = doc.Descendants("item");

        foreach (var item in items)
        {
            try
            {
                var title = item.Element("title")?.Value ?? "";
                var description = item.Element("description")?.Value ?? "";
                var geoLat = item.Descendants().FirstOrDefault(e => e.Name.LocalName == "lat")?.Value;
                var geoLon = item.Descendants().FirstOrDefault(e => e.Name.LocalName == "long")?.Value;

                // Parse magnitude from title (format: "Magnitud X.X ...")
                double mag = 0;
                var magMatch = System.Text.RegularExpressions.Regex.Match(title, @"(\d+\.?\d*)\s");
                if (magMatch.Success) double.TryParse(magMatch.Groups[1].Value, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out mag);

                if (geoLat != null && geoLon != null &&
                    double.TryParse(geoLat, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat) &&
                    double.TryParse(geoLon, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lon))
                {
                    var pubDate = item.Element("pubDate")?.Value;
                    DateTime.TryParse(pubDate, out var time);

                    earthquakes.Add(new Earthquake
                    {
                        Id = $"ssn_{earthquakes.Count}",
                        Magnitude = mag,
                        Place = title,
                        Time = time,
                        Lat = lat,
                        Lon = lon,
                        Depth = 0,
                        Url = item.Element("link")?.Value ?? "",
                        MagType = "SSN",
                        Title = title
                    });
                }
            }
            catch { }
        }

        return earthquakes;
    }
}
