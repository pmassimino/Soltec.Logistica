using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Soltec.Logistica.Model
{
    public class MovPunto
    {
        [Key, DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int IdChofer { get; set; }
        [MaxLength(100)]
        public string Concepto { get; set; }
        public long NumeroComprobante { get; set; }
        public Guid IdTransaccion { get; set; }
        public double Cantidad { get; set; }
        
    }

}
