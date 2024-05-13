﻿using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class SupplierDomain
    {
        public int? Id { get; set; }
        public string? SupplierTypeName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }

}
