using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class Produtos
    {
        public required int Id { get; set; }
        public required string Cidade { get; set; }
        public required string NomeDistribuidor { get; set; }
        public required string NomeResponsavel { get; set; }
        public required string Telefone { get; set; }

    }
}
