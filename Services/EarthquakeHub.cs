using Microsoft.AspNetCore.SignalR;

namespace BalizasV16.Services;

public class EarthquakeHub : Hub
{
    public async Task RequestEarthquakes(double? minMag, string? timeRange, bool? mexicoOnly)
    {
        var quakes = EarthquakeService.CurrentEarthquakes.AsEnumerable();

        if (minMag.HasValue && minMag > 0)
            quakes = quakes.Where(e => e.Magnitude >= minMag.Value);

        if (!string.IsNullOrEmpty(timeRange) && timeRange != "week")
        {
            var now = DateTime.UtcNow;
            quakes = timeRange switch
            {
                "hour" => quakes.Where(e => e.Time >= now.AddHours(-1)),
                "day" => quakes.Where(e => e.Time >= now.AddDays(-1)),
                "3days" => quakes.Where(e => e.Time >= now.AddDays(-3)),
                _ => quakes
            };
        }

        if (mexicoOnly == true)
        {
            // Mexico bounding box approximately
            quakes = quakes.Where(e => e.Lat >= 14 && e.Lat <= 33 && e.Lon >= -118 && e.Lon <= -86);
        }

        await Clients.Caller.SendAsync("EarthquakesUpdated", quakes.ToList());
    }
}
