using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Soltec.Logistica.Model
{
    public class Viaje
    {
        [Key, DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        [ForeignKey("Chofer")]
        public int IdChofer { get; set; }        
        public int IdTipo { get; set; }
        [MaxLength(60)]
        public string Concepto { get; set; }
        public long NumeroComprobante { get; set; }
        public Guid IdTransaccion { get; set; }
        public decimal Distancia { get; set; }
        [MaxLength(60)]
        public string Estado { get; set; } 
        public virtual Chofer Chofer { get; set; } 

    }
    public class ViajeDTO 
    {
        public DateTime Fecha { get; set; }        
        public int IdChofer { get; set; }
        public int IdTipo { get; set; }
        public string Concepto { get; set; }
        public long NumeroComprobante { get; set; }        
        public decimal Distancia { get; set; }
        public string Estado { get; set; }
    }
    public class TipoViaje 
    {
        [Key, DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(60)]
        public string Nombre { get; set; }
        public double Puntos { get; set; }
    }

}
