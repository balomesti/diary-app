namespace diary_app.Models;

public class GeolocationResult
{
    [System.Text.Json.Serialization.JsonPropertyName("coords")]
    public GeolocationCoords? Coords { get; set; }
}

public class GeolocationCoords
{
    [System.Text.Json.Serialization.JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}
