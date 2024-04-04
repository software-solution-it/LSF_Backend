using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class Supplier
    {
        public int Id { get; set; }
        public string City { get; set; }
        public string SupplierName { get; set; }
        public string SupplierResponsible { get; set; }
        public string Phone { get; set; }

    }

    public class SupplierModel
    {
        public string City { get; set; }
        public string SupplierName { get; set; }
        public string SupplierResponsible { get; set; }
        public string Phone { get; set; }

    }
}
