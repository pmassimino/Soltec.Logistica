using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.util;

namespace Soltec.Logistica.Model
{
    public class Chofer
    {
        [Key, DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(100),Required]
        public string Nombre { get; set; }
        public long NumeroDocumento { get; set; }
        [MaxLength(100)]
        public string Telefono { get; set; }
        [MaxLength(100)]
        public string Email {  get; set; }
        [MaxLength(20), Required]
        public string Patente { get; set; }
        [MaxLength(20)]
        public string PatenteAcoplado { get; set; }
        
    }
}
