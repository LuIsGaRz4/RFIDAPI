using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RFID_API.Models
{
    public class RFIDUsuarios
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdUsuario { get; set; }
        public string? Nombre { get; set; }
        public string? Rol { get; set; }
        public string? IdTarjeta { get; set; }
    }
}
