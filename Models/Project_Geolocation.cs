using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace LSF.Models
{
    public class ProjectGeolocation
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        [ForeignKey("Geolocation")]
        public int? GeolocationId { get; set; }
        public virtual Geolocation? Geolocation { get; set; }
    }

    public class ProjectGeolocationModel
    {
        public int ProjectId { get; set; }
        public int GeolocationId { get; set; }
    }
}