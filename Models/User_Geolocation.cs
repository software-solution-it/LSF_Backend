using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace LSF.Models
{
    public class UserGeolocation
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User? User { get; set; }

        [ForeignKey("Geolocation")]
        public int GeolocationId { get; set; }
        public virtual Geolocation? Geolocation { get; set; }
    }

    public class UserGeolocationModel
    {
        public int UserId { get; set; }
        public int GeolocationId { get; set; }
    }
}