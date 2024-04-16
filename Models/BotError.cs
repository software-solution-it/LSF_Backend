using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class BotError
    {
        public int? Id { get; set; }
        public string? Visor { get; set; }
        public string? Description { get; set; }
        public string? Cause { get; set; }

    }

}
