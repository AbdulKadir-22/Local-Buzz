using System.Net;

namespace LocalBuzz.Services;

public static class GeoHelper
{
    private const double EarthRadiusKm = 6371.0;

    /// <summary>
    /// Calculates distance in kilometers between two lat/lng points using the Haversine formula.
    /// </summary>
    public static double HaversineDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var rLat1 = ToRadians(lat1);
        var rLat2 = ToRadians(lat2);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(rLat1) * Math.Cos(rLat2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    /// <summary>
    /// Returns a direct Google Maps navigation URL for the shop.
    /// </summary>
    public static string GetDirectionsUrl(double? lat, double? lng, string name, string address, string locality, string city)
    {
        if (lat.HasValue && lng.HasValue)
        {
            return $"https://www.google.com/maps/dir/?api=1&destination={lat.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)},{lng.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        }

        var destination = WebUtility.UrlEncode($"{name}, {address}, {locality}, {city}");
        return $"https://www.google.com/maps/dir/?api=1&destination={destination}";
    }

    public static string FormatDistance(double distanceKm)
    {
        if (distanceKm < 1.0)
        {
            var meters = (int)Math.Round(distanceKm * 1000);
            return $"{meters} m";
        }
        return $"{distanceKm:0.0} km";
    }
}
