using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace LSF.Models
{
    public class ProjectPoint
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }

        [ForeignKey("Point")]
        public int? PointId { get; set; }
        public virtual Point? Point { get; set; }
    }

    public class ProjectPointModel
    {
        public int ProjectId { get; set; }
        public int PointId { get; set; }
    }
}