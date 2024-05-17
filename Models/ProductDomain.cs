﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LSF.Models
{
    public class ProductDomain
    {
        public int Id { get; set; }
        public string? ProductName { get; set; }
        public string? ProductBrand { get; set; }
        public int? WasherType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("SupplierDomain")]
        public int? SupplierType { get; set; }
        public virtual SupplierDomain? SupplierDomain { get; set; }
    }

}
