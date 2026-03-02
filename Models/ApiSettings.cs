namespace BalizasV16.Models;

public class ApiSettings
{
    public string? GooglePlacesApiKey { get; set; }
    public string? YelpApiKey { get; set; }
    public string? FoursquareApiKey { get; set; }
    public string? TomTomApiKey { get; set; }

    public bool HasGooglePlaces => !string.IsNullOrWhiteSpace(GooglePlacesApiKey);
    public bool HasYelp => !string.IsNullOrWhiteSpace(YelpApiKey);
    public bool HasFoursquare => !string.IsNullOrWhiteSpace(FoursquareApiKey);
    public bool HasTomTom => !string.IsNullOrWhiteSpace(TomTomApiKey);
}
