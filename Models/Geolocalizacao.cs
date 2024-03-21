﻿using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class Geolocalizacao
    {
        public required int Id { get; set; }
        public int CodigoAluno { get; set; }
        public DateTime DataPedido { get; set; }
        public string? EndPedido { get; set; }
        public string? ResponsavelPedido { get; set; }
        public DateTime DataResponsavelPedido { get; set; }
        public bool? Status { get; set; }

    }
}
