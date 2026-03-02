using System.Xml.Linq;
using BalizasV16.Models;
using Microsoft.AspNetCore.SignalR;

namespace BalizasV16.Services;

public class BalizaService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHubContext<BalizaHub> _hubContext;
    private readonly ILogger<BalizaService> _logger;
    private readonly TimeSpan _pollInterval = TimeSpan.FromMinutes(2);

    private const string DgtFeedUrl = "https://nap.dgt.es/datex2/v3/dgt/SituationPublication/datex2_v36.xml";

    public static List<Baliza> CurrentBalizas { get; private set; } = new();

    public BalizaService(IHttpClientFactory httpClientFactory, IHubContext<BalizaHub> hubContext, ILogger<BalizaService> logger)
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
                var balizas = await FetchBalizasAsync(stoppingToken);
                CurrentBalizas = balizas;
                _logger.LogInformation("Fetched {Count} active V16 beacons", balizas.Count);
                await _hubContext.Clients.All.SendAsync("BalizasUpdated", balizas, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching V16 beacons from DGT");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }
    }

    private async Task<List<Baliza>> FetchBalizasAsync(CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("DGT");
        var response = await client.GetStringAsync(DgtFeedUrl, ct);

        return ParseDatexII(response);
    }

    private List<Baliza> ParseDatexII(string xml)
    {
        var balizas = new List<Baliza>();
        var doc = XDocument.Parse(xml);

        var records = doc.Descendants()
            .Where(e => e.Name.LocalName == "situationRecord");

        int index = 0;
        foreach (var record in records)
        {
            var causeType = record.Descendants().FirstOrDefault(e => e.Name.LocalName == "causeType")?.Value;
            var status = record.Descendants().FirstOrDefault(e => e.Name.LocalName == "validityStatus")?.Value;

            if (causeType != "vehicleObstruction" || status != "active")
                continue;

            var latEl = record.Descendants().FirstOrDefault(e => e.Name.LocalName == "latitude");
            var lonEl = record.Descendants().FirstOrDefault(e => e.Name.LocalName == "longitude");

            if (latEl == null || lonEl == null)
                continue;

            if (!double.TryParse(latEl.Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat) ||
                !double.TryParse(lonEl.Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lon))
                continue;

            var roadName = record.Descendants().FirstOrDefault(e => e.Name.LocalName == "roadName")?.Value ?? "";
            var municipality = record.Descendants().FirstOrDefault(e => e.Name.LocalName == "municipality")?.Value ?? "";
            var province = record.Descendants().FirstOrDefault(e => e.Name.LocalName == "province")?.Value ?? "";
            var obstType = record.Descendants().FirstOrDefault(e => e.Name.LocalName == "vehicleObstructionType")?.Value ?? "Vehículo detenido";
            var startTime = record.Descendants().FirstOrDefault(e => e.Name.LocalName == "overallStartTime")?.Value ?? "N/A";

            var locationParts = new[] { roadName, municipality, province }.Where(p => !string.IsNullOrWhiteSpace(p));

            balizas.Add(new Baliza
            {
                Id = record.Attribute("id")?.Value ?? $"beacon_{index}",
                Lat = lat,
                Lon = lon,
                Time = startTime,
                Type = obstType,
                Location = locationParts.Any() ? string.Join(", ", locationParts) : "Ubicación desconocida"
            });

            index++;
        }

        return balizas;
    }
}
