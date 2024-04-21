using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LSF.Models
{
    public class UserProduct
    {
        public int Id { get; set; }
        public int? Quantity { get; set; }

        [ForeignKey("User")]
        public int? UserId { get; set; }
        public virtual User? User { get; set; }

        [ForeignKey("ProductDomain")]
        public int? ProductId { get; set; }
        public virtual ProductDomain? ProductDomain { get; set; }

        [ForeignKey("SupplierDomain")]
        public int? SupplierType { get; set; }
        public virtual SupplierDomain? SupplierDomain { get; set; }
    }

}
