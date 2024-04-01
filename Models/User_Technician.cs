using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace LSF.Models
{
    public class UserTechnician
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User? User { get; set; }

        [ForeignKey("Technician")]
        public int TechnicianId { get; set; }
        public virtual Technician? Technician { get; set; }
    }

    public class UserTechnicianModel
    {
        public int UserId { get; set; }
        public int TechnicianId { get; set; }
    }
}