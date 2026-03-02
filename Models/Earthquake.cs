namespace BalizasV16.Models;

public class Earthquake
{
    public string Id { get; set; } = string.Empty;
    public double Magnitude { get; set; }
    public string Place { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
    public double Depth { get; set; }
    public string Url { get; set; } = string.Empty;
    public int? Felt { get; set; }
    public string? Alert { get; set; }
    public int Tsunami { get; set; }
    public string MagType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Significance { get; set; }

    public string SeverityClass => Magnitude switch
    {
        >= 7.0 => "extreme",
        >= 5.0 => "major",
        >= 4.0 => "moderate",
        >= 2.5 => "light",
        _ => "minor"
    };

    public string SeverityColor => Magnitude switch
    {
        >= 7.0 => "#ff0000",
        >= 5.0 => "#ff6600",
        >= 4.0 => "#ffaa00",
        >= 2.5 => "#ffdd00",
        _ => "#88cc00"
    };

    public int MarkerSize => Magnitude switch
    {
        >= 7.0 => 40,
        >= 5.0 => 32,
        >= 4.0 => 24,
        >= 2.5 => 18,
        _ => 12
    };
}
