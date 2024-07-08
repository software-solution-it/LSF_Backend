using Microsoft.AspNetCore.Identity;

namespace LSF.Models
{
    public class User
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? UserName { get; set; }

        public string? Phone { get; set; }

        public string? ThreadId { get; set; }

        public bool? FirstAccess { get; set; }

        public string? Email { get; set; }
        public string? Password { get; set; }
        public byte[]? UserImage { get; set; }
        public virtual ICollection<Project>? Projects { get; set; }
        public bool isActive { get; set; }
        public int? RecoveryCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<UserRole>? UserRoles { get; set; }
    }

    public class UserModel
    {
        public string? Name { get; set; }

        public string? UserName { get; set; }

        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public byte[]? UserImage { get; set; }
    }

    public class UserModelRegister
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}