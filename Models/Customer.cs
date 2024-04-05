using System.ComponentModel.DataAnnotations.Schema;

namespace LSF.Models
{
    public class Customer
    {
        public User User { get; set; }
        public Geolocation Geolocation { get; set; }
        public Point Point { get; set; }
        public Supplier Supplier { get; set; }
        public Technician Technician { get; set; }
    }

}