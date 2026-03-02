namespace BalizasV16.Models;

public class Baliza
{
    public string Id { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lon { get; set; }
    public string Time { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}
