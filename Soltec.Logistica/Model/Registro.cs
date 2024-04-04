using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Soltec.Logistica.Model
{
    public class Registro
    {
        [Key, DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        [ForeignKey("Chofer")]
        public int IdChofer { get; set; }
        [MaxLength(100)]        
        public virtual Chofer Chofer { get; set; } 
        public virtual ICollection<EstadoRegistro> Estados { get; set; } = new List<EstadoRegistro>();
    }

    public class EstadoRegistro
    {
        [Key, Column(Order = 0), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Key, Column(Order = 1), Required, ForeignKey("Registro")]
        public int IdRegistro { get; set; }

        public virtual Registro Registro { get; set; }

        public DateTime Fecha { get; set; }

        [MaxLength(100)]
        public string Estado { get; set; }
    }


    public class RegistroDTO
    { 
        public int Id { get; set; }
        public DateTime Fecha { get; set; }      
        public int IdChofer { get; set; }
        [MaxLength(100)]        
        public virtual string Estado { get; set; }
    }
    public class RegistroView 
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }        
        public int IdChofer { get; set; }        
        public string NombreChofer { get; set; }        
        public string Patente { get; set; }
        public string PatenteAcoplado { get; set; }
        public string Estado { get; set; }
        public int CantidadCorto { get; set; }
        public int CantidadLargo { get; set; }
        public decimal Puntos { get; set; }
        public virtual Chofer Chofer { get; set; }

    }


}
