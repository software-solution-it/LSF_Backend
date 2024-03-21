using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class Marca
    {
        public required int Id { get; set; }
        public required string NomeMarca { get; set; }
        public required bool Status { get; set; }

    }
}
