using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class Role
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public virtual ICollection<UserRole>? UserRoles { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
