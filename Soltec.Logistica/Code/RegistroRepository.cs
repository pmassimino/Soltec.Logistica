using Microsoft.EntityFrameworkCore;
using Soltec.Logistica.Data;
using Soltec.Logistica.Model;
using Soltec.Logistica.Service;

namespace Soltec.Logistica.Code
{
    public interface IRegistroRepository : IGenericRepository<Registro>
    {
        // Agregar métodos específicos para el repositorio de estado de registro si es necesario
        // Por ejemplo:
        void InsertEstadoRegistro(EstadoRegistro estadoRegistro);
    }

    public class RegistroRepository : GenericRepository<Registro>, IRegistroRepository
    {
        public RegistroRepository() : base() { }

        public RegistroRepository(LogisticaContext context) : base(context) { }

        // Implementación del método para insertar un estado de registro
        public void InsertEstadoRegistro(EstadoRegistro estadoRegistro)
        {
            this.context.Entry(estadoRegistro).State = EntityState.Added;
        }
    }

}
