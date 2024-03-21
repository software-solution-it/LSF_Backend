using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class Tecnicos
    {
        public required int Id { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string Telefone { get; set; }
        public required string Cidade { get; set; }
        public required string Estado { get; set; }
        public bool Ativo { get; set; }
    }
}