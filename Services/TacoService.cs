using System.Text.Json;
using System.Text.Json.Serialization;
using BalizasV16.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace BalizasV16.Services;

public class TacoService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TacoService> _logger;
    private readonly IOptionsMonitor<ApiSettings> _apiSettings;

    private static readonly (string Name, double Lat, double Lon)[] Cities =
    [
        ("Ciudad de México", 19.4326, -99.1332),
        ("Guadalajara", 20.6597, -103.3496),
        ("Monterrey", 25.6866, -100.3161),
        ("Puebla", 19.0414, -98.2063),
        ("Tijuana", 32.5149, -117.0382),
        ("León", 21.1221, -101.6860),
        ("Mérida", 20.9674, -89.5926),
        ("Oaxaca", 17.0732, -96.7266)
    ];

    private const string OverpassUrl = "https://overpass-api.de/api/interpreter";
    private List<TacoStand> _cachedStands = new();
    private DateTime _lastFetch = DateTime.MinValue;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

    public TacoService(IHttpClientFactory httpClientFactory, ILogger<TacoService> logger, IOptionsMonitor<ApiSettings> apiSettings)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _apiSettings = apiSettings;
    }

    public async Task<List<TacoStand>> GetTacoStandsAsync(string? city = null, string? tacoType = null)
    {
        if (_cachedStands.Count == 0 || DateTime.UtcNow - _lastFetch > _cacheDuration)
        {
            await FetchAllProvidersAsync();
        }

        var results = _cachedStands.AsEnumerable();

        if (!string.IsNullOrEmpty(tacoType) && tacoType != "all")
        {
            results = results.Where(t =>
                t.TacoType.Contains(tacoType, StringComparison.OrdinalIgnoreCase) ||
                t.Cuisine.Contains(tacoType, StringComparison.OrdinalIgnoreCase) ||
                t.Name.Contains(tacoType, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(city) && city != "all")
        {
            var cityInfo = Cities.FirstOrDefault(c => c.Name.Equals(city, StringComparison.OrdinalIgnoreCase));
            if (cityInfo != default)
            {
                results = results.Where(t =>
                    GetDistanceKm(t.Lat, t.Lon, cityInfo.Lat, cityInfo.Lon) < 30);
            }
        }

        return results.ToList();
    }

    public List<string> GetActiveProviders()
    {
        var providers = new List<string> { "OpenStreetMap (Overpass)" };
        var settings = _apiSettings.CurrentValue;
        if (settings.HasYelp) providers.Add("Yelp Fusion");
        if (settings.HasGooglePlaces) providers.Add("Google Places");
        if (settings.HasFoursquare) providers.Add("Foursquare");
        return providers;
    }

    private async Task FetchAllProvidersAsync()
    {
        var allStands = new List<TacoStand>();
        var tasks = new List<Task<List<TacoStand>>>();

        // Overpass always runs
        tasks.Add(FetchFromOverpassAsync());

        var settings = _apiSettings.CurrentValue;
        if (settings.HasYelp)
            tasks.Add(FetchFromYelpAsync(settings.YelpApiKey!));
        if (settings.HasGooglePlaces)
            tasks.Add(FetchFromGooglePlacesAsync(settings.GooglePlacesApiKey!));
        if (settings.HasFoursquare)
            tasks.Add(FetchFromFoursquareAsync(settings.FoursquareApiKey!));

        var results = await Task.WhenAll(tasks);
        foreach (var result in results)
            allStands.AddRange(result);

        // Deduplicate by proximity (within 50m = same place)
        _cachedStands = DeduplicateByProximity(allStands, 0.05);
        _lastFetch = DateTime.UtcNow;
        _logger.LogInformation("Fetched {Count} taco stands from {Providers} providers", _cachedStands.Count, results.Length);
    }

    // ── Overpass/OSM Provider ──
    private async Task<List<TacoStand>> FetchFromOverpassAsync()
    {
        var client = _httpClientFactory.CreateClient("Overpass");
        var stands = new List<TacoStand>();

        var query = @"
[out:json][timeout:60];
area[""name""=""México""][admin_level=2]->.mx;
(
  node[""amenity""=""fast_food""][""cuisine""~""taco|mexican"",i](area.mx);
  node[""amenity""=""restaurant""][""cuisine""~""taco|mexican"",i](area.mx);
  node[""shop""=""food""][""name""~""taco|taquería|taqueria"",i](area.mx);
  node[""amenity""=""fast_food""][""name""~""taco|taquería|taqueria"",i](area.mx);
  node[""amenity""=""restaurant""][""name""~""taco|taquería|taqueria"",i](area.mx);
);
out body;";

        try
        {
            var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("data", query) });
            var response = await client.PostAsync(OverpassUrl, content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OverpassResult>(json);

            if (result?.Elements != null)
            {
                foreach (var el in result.Elements)
                {
                    var name = el.Tags?.GetValueOrDefault("name") ?? "Taquería";
                    var hours = el.Tags?.GetValueOrDefault("opening_hours") ?? "";
                    var cuisine = el.Tags?.GetValueOrDefault("cuisine") ?? "";
                    var addr = el.Tags?.GetValueOrDefault("addr:street") ?? "";

                    stands.Add(new TacoStand
                    {
                        Id = el.Id,
                        Name = name,
                        Lat = el.Lat,
                        Lon = el.Lon,
                        OpeningHours = hours,
                        Cuisine = cuisine,
                        TacoType = ClassifyTacoType(name, cuisine),
                        IsOpenLateNight = IsOpenAfter11PM(hours),
                        Address = addr
                    });
                }
            }
            _logger.LogInformation("Overpass: {Count} taco stands", stands.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching from Overpass API");
        }
        return stands;
    }

    // ── Yelp Fusion Provider ──
    private async Task<List<TacoStand>> FetchFromYelpAsync(string apiKey)
    {
        var client = _httpClientFactory.CreateClient("Yelp");
        var stands = new List<TacoStand>();

        foreach (var city in Cities)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"businesses/search?term=tacos&latitude={city.Lat}&longitude={city.Lon}&radius=10000&categories=mexican&limit=50");
                request.Headers.Add("Authorization", $"Bearer {apiKey}");

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Yelp API returned {Status} for {City}", response.StatusCode, city.Name);
                    continue;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var businesses = doc.RootElement.GetProperty("businesses");

                foreach (var biz in businesses.EnumerateArray())
                {
                    var coords = biz.GetProperty("coordinates");
                    var name = biz.GetProperty("name").GetString() ?? "Taquería";
                    var lat = coords.GetProperty("latitude").GetDouble();
                    var lon = coords.GetProperty("longitude").GetDouble();

                    var addr = "";
                    if (biz.TryGetProperty("location", out var loc) && loc.TryGetProperty("address1", out var a))
                        addr = a.GetString() ?? "";

                    var rating = biz.TryGetProperty("rating", out var r) ? r.GetDouble() : 0;

                    stands.Add(new TacoStand
                    {
                        Id = stands.Count + 100000,
                        Name = $"{name} ⭐{rating}",
                        Lat = lat,
                        Lon = lon,
                        Cuisine = "mexican",
                        TacoType = ClassifyTacoType(name, "mexican"),
                        IsOpenLateNight = false, // Yelp search doesn't give hours
                        Address = addr
                    });
                }
                _logger.LogInformation("Yelp: {Count} results for {City}", businesses.GetArrayLength(), city.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Yelp for {City}", city.Name);
            }
        }
        return stands;
    }

    // ── Google Places Provider ──
    private async Task<List<TacoStand>> FetchFromGooglePlacesAsync(string apiKey)
    {
        var client = _httpClientFactory.CreateClient("GooglePlaces");
        var stands = new List<TacoStand>();

        foreach (var city in Cities.Take(3)) // Limit to 3 cities to conserve quota
        {
            try
            {
                var body = JsonSerializer.Serialize(new
                {
                    includedTypes = new[] { "restaurant" },
                    maxResultCount = 20,
                    locationRestriction = new
                    {
                        circle = new
                        {
                            center = new { latitude = city.Lat, longitude = city.Lon },
                            radius = 5000.0
                        }
                    }
                });

                var request = new HttpRequestMessage(HttpMethod.Post, "v1/places:searchNearby");
                request.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                request.Headers.Add("X-Goog-Api-Key", apiKey);
                request.Headers.Add("X-Goog-FieldMask", "places.displayName,places.location,places.currentOpeningHours,places.formattedAddress");

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Google Places returned {Status} for {City}", response.StatusCode, city.Name);
                    continue;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("places", out var places))
                {
                    foreach (var place in places.EnumerateArray())
                    {
                        var name = "Taquería";
                        if (place.TryGetProperty("displayName", out var dn) && dn.TryGetProperty("text", out var txt))
                            name = txt.GetString() ?? "Taquería";

                        if (!name.Contains("taco", StringComparison.OrdinalIgnoreCase) &&
                            !name.Contains("taquería", StringComparison.OrdinalIgnoreCase) &&
                            !name.Contains("taqueria", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var location = place.GetProperty("location");
                        var lat = location.GetProperty("latitude").GetDouble();
                        var lon = location.GetProperty("longitude").GetDouble();

                        var addr = place.TryGetProperty("formattedAddress", out var fa) ? fa.GetString() ?? "" : "";
                        var isOpenNow = false;
                        if (place.TryGetProperty("currentOpeningHours", out var oh) && oh.TryGetProperty("openNow", out var on))
                            isOpenNow = on.GetBoolean();

                        stands.Add(new TacoStand
                        {
                            Id = stands.Count + 200000,
                            Name = name,
                            Lat = lat,
                            Lon = lon,
                            Cuisine = "mexican",
                            TacoType = ClassifyTacoType(name, "mexican"),
                            IsOpenLateNight = isOpenNow,
                            Address = addr
                        });
                    }
                }
                _logger.LogInformation("Google Places: results for {City}", city.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Google Places for {City}", city.Name);
            }
        }
        return stands;
    }

    // ── Foursquare Provider ──
    private async Task<List<TacoStand>> FetchFromFoursquareAsync(string apiKey)
    {
        var client = _httpClientFactory.CreateClient("Foursquare");
        var stands = new List<TacoStand>();

        foreach (var city in Cities)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"places/search?query=tacos&ll={city.Lat},{city.Lon}&radius=10000&categories=13306&limit=50");
                request.Headers.Add("Authorization", apiKey);

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Foursquare returned {Status} for {City}", response.StatusCode, city.Name);
                    continue;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("results", out var results))
                {
                    foreach (var place in results.EnumerateArray())
                    {
                        var name = place.TryGetProperty("name", out var n) ? n.GetString() ?? "Taquería" : "Taquería";
                        var geo = place.GetProperty("geocodes").GetProperty("main");
                        var lat = geo.GetProperty("latitude").GetDouble();
                        var lon = geo.GetProperty("longitude").GetDouble();

                        var addr = "";
                        if (place.TryGetProperty("location", out var loc) && loc.TryGetProperty("formatted_address", out var fa))
                            addr = fa.GetString() ?? "";

                        stands.Add(new TacoStand
                        {
                            Id = stands.Count + 300000,
                            Name = name,
                            Lat = lat,
                            Lon = lon,
                            Cuisine = "mexican",
                            TacoType = ClassifyTacoType(name, "mexican"),
                            IsOpenLateNight = false,
                            Address = addr
                        });
                    }
                }
                _logger.LogInformation("Foursquare: results for {City}", city.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Foursquare for {City}", city.Name);
            }
        }
        return stands;
    }

    // ── Helpers ──

    private static List<TacoStand> DeduplicateByProximity(List<TacoStand> stands, double thresholdKm)
    {
        var deduped = new List<TacoStand>();
        foreach (var stand in stands)
        {
            bool isDupe = deduped.Any(s =>
                GetDistanceKm(s.Lat, s.Lon, stand.Lat, stand.Lon) < thresholdKm &&
                s.Name.Contains(stand.Name.Split(' ')[0], StringComparison.OrdinalIgnoreCase));

            if (!isDupe) deduped.Add(stand);
        }
        return deduped;
    }

    private static string ClassifyTacoType(string name, string cuisine)
    {
        var combined = $"{name} {cuisine}".ToLowerInvariant();

        if (combined.Contains("pastor") || combined.Contains("trompo")) return "pastor";
        if (combined.Contains("suadero")) return "suadero";
        if (combined.Contains("birria")) return "birria";
        if (combined.Contains("carnita")) return "carnitas";
        if (combined.Contains("cabeza")) return "cabeza";
        if (combined.Contains("barbacoa")) return "barbacoa";
        if (combined.Contains("canasta")) return "canasta";
        if (combined.Contains("fish") || combined.Contains("pescado") || combined.Contains("marisco")) return "pescado";
        if (combined.Contains("campechano")) return "campechano";

        return "variado";
    }

    private static bool IsOpenAfter11PM(string openingHours)
    {
        if (string.IsNullOrWhiteSpace(openingHours)) return false;
        if (openingHours.Contains("24/7")) return true;

        try
        {
            var parts = openingHours.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                var timeMatch = System.Text.RegularExpressions.Regex.Match(trimmed, @"(\d{1,2}):(\d{2})\s*-\s*(\d{1,2}):(\d{2})");
                if (timeMatch.Success)
                {
                    var closeHour = int.Parse(timeMatch.Groups[3].Value);
                    var openHour = int.Parse(timeMatch.Groups[1].Value);

                    if (closeHour < openHour) return true;
                    if (closeHour >= 23) return true;
                    if (openHour >= 20 && closeHour <= 6) return true;
                }
            }
        }
        catch { }

        return false;
    }

    private static double GetDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371.0;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    public static string[] GetTacoTypes() =>
        ["all", "pastor", "suadero", "birria", "carnitas", "cabeza", "barbacoa", "canasta", "pescado", "campechano", "variado"];

    public static string[] GetCities() =>
        ["all", "Ciudad de México", "Guadalajara", "Monterrey", "Puebla", "Tijuana", "León", "Mérida", "Oaxaca"];
}

// Overpass API response models
public class OverpassResult
{
    [JsonPropertyName("elements")]
    public List<OverpassElement> Elements { get; set; } = new();
}

public class OverpassElement
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }

    [JsonPropertyName("tags")]
    public Dictionary<string, string>? Tags { get; set; }
}
