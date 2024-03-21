using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class Maquina
    {
        public required int Id { get; set; }
        public required string CodigoFornecedor { get; set; }
        public required string Modelo { get; set; }
        public string? Capacidade { get; set; }
        public int? CodigoMarca { get; set; }

    }
}
