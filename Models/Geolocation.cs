using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class Geolocation
    {
        public int? Id { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }

    public class GeolocationModel
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Address { get; set; }

    }
}
