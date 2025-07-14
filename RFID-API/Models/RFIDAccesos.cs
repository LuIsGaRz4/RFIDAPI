using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RFID_API.Models
{
    public class RFIDAccesos
    {

        [Key]
        [Column("IdAccesos")]
        public bool IdAccesos { get; set; }
        public string? Descripcion { get; set; }

        public ICollection<RFIDRegistros>? Registros { get; set; }
    }
}
