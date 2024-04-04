using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace LSF.Models
{
    public class UserPoint
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User? User { get; set; }

        [ForeignKey("Point")]
        public int? PointId { get; set; }
        public virtual Point? Point { get; set; }
    }

    public class UserPointModel
    {
        public int UserId { get; set; }
        public int PointId { get; set; }
    }
}