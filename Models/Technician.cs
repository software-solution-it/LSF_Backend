namespace LSF.Models
{
    public class Technician
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public bool? Active { get; set; }
    }

    public class TechnicianModel
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public bool? Active { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}