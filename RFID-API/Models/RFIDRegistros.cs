using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RFID_API.Models
{
    public class RFIDRegistros
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }
        public string? IdRegistro { get; set; }


        [ForeignKey("Acceso")]
        public bool IdAccesos { get; set; } 

        public string? Nombre { get; set; }

        public DateTime? Fecha { get; set; }

        [JsonIgnore]
        public RFIDAccesos? Acceso { get; set; }
    }
}
