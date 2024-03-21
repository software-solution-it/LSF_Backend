using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class MaquinaLavar
    {
        public required int Id { get; set; }
        public required string CodigoMaquina { get; set; }
        public int? Quantidade { get; set; }
        public bool? Status { get; set; }

    }
}
