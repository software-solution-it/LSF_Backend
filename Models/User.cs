using Microsoft.AspNetCore.Identity;

namespace LSF.Models
{
    public class User : IdentityUser
    {
        public byte[]? UserImage { get; set; }
        public byte[]? Comprovante { get; set; }
    }
}