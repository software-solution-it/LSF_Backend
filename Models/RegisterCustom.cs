using Microsoft.AspNetCore.Identity;

namespace LSF.Models
{
    public class RegisterCustom
    {
        public required string Email { get; init; }
        public required string Password { get; init; }
    }
}