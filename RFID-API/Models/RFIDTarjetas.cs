using System.ComponentModel.DataAnnotations;

namespace RFID_API.Models
{
    public class RFIDTarjetas
    {
        [Key]
        public string? IdTarjeta { get; set; }
        public string? Nombre { get; set; }
    }

}
