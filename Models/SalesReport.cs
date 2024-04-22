using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LSF.Models
{
    public class SalesReport
    {
        public int? Id { get; set; }
        [ForeignKey("User")]
        public int? UserId { get; set; }
        public virtual User? User { get; set; }
        public string Laundry { get; set; }
        public string? NameClient { get; set; }
        public DateTime SellDate { get; set; }
        public string Interprise { get; set; }
        public string InterpriseDocument { get; set; }
        public string Equipment { get; set; }
        public bool Situation { get; set; }
        public string PaymentType { get; set; }
        public double Value { get; set; }
        public double ValueWithNoDiscount { get; set; }
        public string? Provider { get; set; }
        public string? Acquirer { get; set; }
        public string? CardFlag { get; set; }
        public string? CardType { get; set; }
        public string CardNumber { get; set; }
        public string? Authorizer { get; set; }
        public string? Voucher { get; set; }
        public string? VoucherCategory { get; set; }
        public string? Cupom { get; set; }
        public string? CPFClient { get; set; }
        public string Requisition { get; set; }
        public string? CupomRequisition { get; set; }
        public string? CodeAuthSender { get; set; }
        public string? Error { get; set; }
        public string? ErrorDetail { get; set; }

    }
}
