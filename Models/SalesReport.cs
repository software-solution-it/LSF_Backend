using System;
using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class SalesReport
    {
        [Key]
        public int? Id { get; set; }

        public int? UserId { get; set; }

        [StringLength(255)]
        public string? Laundry { get; set; }

        public DateTime SellDate { get; set; }

        [StringLength(255)]
        public string? Interprise { get; set; }

        [StringLength(20)]
        public string? InterpriseDocument { get; set; }

        [StringLength(255)]
        public string? Equipment { get; set; }
        public bool? Situation { get; set; }

        [StringLength(50)]
        public string? PaymentType { get; set; }

        public double? Value { get; set; }


        public double? ValueWithNoDiscount { get; set; }

        [StringLength(255)]
        public string? Provider { get; set; }

        [StringLength(255)]
        public string? Acquirer { get; set; }

        [StringLength(50)]
        public string? CardFlag { get; set; }

        [StringLength(50)]
        public string? CardType { get; set; }

        [StringLength(50)]
        public string? CardNumber { get; set; }

        [StringLength(50)]
        public string? Authorizer { get; set; }

        [StringLength(50)]
        public string? Voucher { get; set; }

        [StringLength(50)]
        public string? VoucherCategory { get; set; }

        [StringLength(50)]
        public string? Cupom { get; set; }

        [StringLength(20)]
        public string? CPFClient { get; set; }

        [StringLength(255)]
        public string? NameClient { get; set; }

        [StringLength(50)]
        public string? Requisition { get; set; }

        [StringLength(50)]
        public string? CupomRequisition { get; set; }

        [StringLength(50)]
        public string? CodeAuthSender { get; set; }

        [StringLength(50)]
        public string? Error { get; set; }

        [StringLength(255)]
        public string? ErrorDetail { get; set; }
    }
}
