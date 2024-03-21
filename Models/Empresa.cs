using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class Empresa
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string Documento { get; set; }
        public required string Cidade { get; set; }
        public string? Contato_1 { get; set; }
        public string? Contato_2 { get; set; }
        public string? Contato_3 { get; set; }
        public string? Especialidade { get; set; }
        public required bool Ativo { get; set; }

    }
}
