using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class UserToken
    {
        public int Id { get; set; }
        public int? User_Id { get; set; }
        public String? ResetToken { get; set; }
        public DateTime? CreatedAt { get; set; }

    }
}