using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LSF.Models
{
    public class Supplier
    {
        public int Id { get; set; }
        public string? City { get; set; }
        public string SupplierName { get; set; }
        public string Phone { get; set; }

        [ForeignKey("SupplierDomain")]
        public int? SupplierType { get; set; }
        public virtual SupplierDomain? SupplierDomain { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }

    public class SupplierModel
    {
        public string City { get; set; }
        public string SupplierName { get; set; }
        public string Phone { get; set; }
        public int SupplierType { get; set; }

    }
}
