using Microsoft.AspNetCore.Identity;

namespace LSF.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public int? EmailConfirmed { get; set; }
        public string? Password { get; set; }
        public byte[]? Comprovante { get; set; }
        public byte[]? UserImage { get; set; }
        public int? RecoveryCode { get; set; }
        public virtual ICollection<UserRole>? UserRoles { get; set; }
    }

    public class UserModel
    {
        public string? Email { get; set; }
        public int? EmailConfirmed { get; set; }
        public string? Password { get; set; }
        public byte[]? Comprovante { get; set; }
        public byte[]? UserImage { get; set; }
        public virtual ICollection<UserRole>? UserRoles { get; set; }
    }

    public class UserModelRegister
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}