namespace BalizasV16.Models;

public class TacoStand
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lon { get; set; }
    public string OpeningHours { get; set; } = string.Empty;
    public string Cuisine { get; set; } = string.Empty;
    public string TacoType { get; set; } = string.Empty;
    public bool IsOpenLateNight { get; set; }
    public int CommunityVotes { get; set; }
    public string Address { get; set; } = string.Empty;
}
