using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class Role
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public virtual ICollection<UserRole>? UserRoles { get; set; }
    }

    public class UserRole
    {
        public int? UserId { get; set; }
        public virtual User? User { get; set; }

        public int? RoleId { get; set; }
        public virtual Role? Role { get; set; }
    }
}
