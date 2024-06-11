using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class Geolocation
    {
        public int? Id { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }

    public class GeolocationModel
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Address { get; set; }

    }
}
