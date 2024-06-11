using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LSF.Models
{
    public class ProjectProduct
    {
        public int Id { get; set; }
        public int? Quantity { get; set; }

        public int? ProjectId { get; set; }

        [ForeignKey("ProductDomain")]
        public int? ProductId { get; set; }
        public virtual ProductDomain? ProductDomain { get; set; }

        [ForeignKey("SupplierDomain")]
        public int? SupplierType { get; set; }
        public virtual SupplierDomain? SupplierDomain { get; set; }
    }

}
