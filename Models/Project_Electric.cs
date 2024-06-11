using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LSF.Models
{
    public class ProjectElectric
    {
        public int Id { get; set; }
        public int? ProjectId { get; set; }
        public string? Voltage { get; set; }
        public string Network { get; set; }

    }

}
