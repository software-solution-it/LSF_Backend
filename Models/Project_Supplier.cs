using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace LSF.Models
{
    public class ProjectSupplier
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        [ForeignKey("Supplier")]
        public int? SupplierId { get; set; }
        public virtual Supplier? Supplier { get; set; }
    }

    public class ProjectSupplierModel
    {
        public int ProjectId { get; set; }
        public int SupplierId { get; set; }
    }
}