using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace LSF.Models
{
    public class ProjectFile
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int? FileId { get; set; }
        public int? ConfirmedReceipt { get; set; }
        public string? ReceiptDeclinedReason { get; set; }

    }

}