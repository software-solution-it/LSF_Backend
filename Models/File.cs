using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace LSF.Models
{
    [Table("File")]
    public class FileModel
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? Folder { get; set; }
        public string? FileType { get; set; }

    }

}