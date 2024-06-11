using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace LSF.Models
{
    public class ProjectTechnician
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int ProjectId { get; set; }

        [ForeignKey("Technician")]
        public int? TechnicianId { get; set; }
        public virtual Technician? Technician { get; set; }
    }

    public class ProjectTechnicianModel
    {
        public int ProjectId { get; set; }
        public int TechnicianId { get; set; }
    }
}