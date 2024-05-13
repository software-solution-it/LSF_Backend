using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class Point
    {
        public int Id { get; set; }
        public string? Width { get; set; }
        public string? Length { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PointModel
    {
        public string? Width { get; set; }
        public string? Length { get; set; }
    }
}
