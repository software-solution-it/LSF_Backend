
using System.ComponentModel.DataAnnotations.Schema;

namespace LSF.Models
{
    public class UserInauguration
    {
        public int? Id { get; set; }

        [ForeignKey("User")]
        public int? UserId { get; set; }
        public virtual User? User { get; set; }

        [ForeignKey("Inauguration")]
        public int? InaugurationId { get; set; }
        public virtual Inauguration? Inauguration { get; set; }
    }

}
