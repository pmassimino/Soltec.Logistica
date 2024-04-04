using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.util;

namespace Soltec.Logistica.Model
{
    public class Transporte
    {
        [Key, DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(100)]
        public string Nombre { get; set; }
        public long NumeroDocumento { get; set; }
        [MaxLength(100)]
        public string Telefono { get; set; }
        [MaxLength(100)]
        public string Email {  get; set; }
        public virtual IList<TransporteChofer> Choferes { get; set; }
    }
    public class TransporteChofer
    {
        [Key, Column(Order = 0), Required, ForeignKey("Transporte")]
        public int IdTransporte { get; set; }
        [Key, Column(Order = 1), Required, ForeignKey("Chofer")]
        public int IdChofer { get; set; }        
        public virtual Transporte Transporte { get; set; }
    }
}
