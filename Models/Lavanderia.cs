using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class Lavanderia
    {
        public required int Id { get; set; }
        public required string CodigoAluno { get; set; }
        public required string Name { get; set; }
        public required string Endereco { get; set; }
        public int? IdMunicipio { get; set; }
        public string? Cep { get; set; }
        public DateTime? DataInauguracao { get; set; }
        public DateTime? DataAtual { get; set; }
        public string? Dimensoes { get; set; }
        public string? Observacoes { get; set; }
        public required bool Ativo { get; set; }

    }
}
