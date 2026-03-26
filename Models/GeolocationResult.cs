namespace diary_app.Models;

public class GeolocationResult
{
    public GeolocationCoords? Coords { get; set; }
}

public class GeolocationCoords
{
    public double latitude { get; set; }
    public double longitude { get; set; }
}
