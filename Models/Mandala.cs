using System.ComponentModel.DataAnnotations;

namespace LSF.Models
{
    public class Mandala
    {
        [Key]
        public int Id { get; set; }

        public bool ChooseLocation { get; set; }

        public bool CloseContract { get; set; }

        public bool PlumbingElectrical { get; set; }

        public bool Drywall { get; set; }

        public bool MDFOptional { get; set; }

        public bool GlassWall { get; set; }

        public bool Machines { get; set; }

        public bool AutomatedComputers { get; set; }

        public bool CardMachine { get; set; }

        public bool PlatesDispensers { get; set; }

        public bool Chemicals { get; set; }

        public bool BrandRegistrationOptional { get; set; }

        public bool Stickers { get; set; }

        public bool EnvironmentDecoration { get; set; }

        public bool SofaTableBasket { get; set; }

        public bool Facade { get; set; }

        public bool AirConditioning { get; set; }

        public bool Internet { get; set; }

        public bool PaperHolder { get; set; }

        public bool AlcoholSprayer { get; set; }

        public bool ClothesFoldersOptional { get; set; }

        public bool Camera { get; set; }

        public bool AirSensor { get; set; }

        public bool MachineAlarm { get; set; }

        public bool WifiSocketAdapter { get; set; }

        public bool DoorLock { get; set; }

        public int userId { get; set; }
    }
}
