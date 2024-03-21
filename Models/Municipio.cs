using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class Municipio
    {
        public required int Id { get; set; }
        public required string Nome { get; set; }
        public required string IdEstado { get; set; }

    }
}
