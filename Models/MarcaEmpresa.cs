using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class MarcaEmpresa
    {
        public required int Id { get; set; }
        public required int CodigoEmpresa { get; set; }
        public required int CodigoMarca { get; set; }

    }
}
