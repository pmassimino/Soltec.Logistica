using Microsoft.EntityFrameworkCore;
using Soltec.Logistica.Data;
using Soltec.Logistica.Model;

namespace Soltec.Logistica.Code
{
    public class DbInitializer
    {
        public static void Seed(LogisticaContext context)
        {
            // context.Database.EnsureCreated();
            //TipoViaje
            if (!context.TipoViaje.Any())
            {                
                List<TipoViaje> list = new List<TipoViaje>();
                TipoViaje item = new TipoViaje() { Nombre = "Corto",Puntos=1 };
                list.Add(item);
                item = new TipoViaje() { Nombre = "Largo", Puntos = -3 };
                list.Add(item);
                context.TipoViaje.AddRange(list);
                context.SaveChanges();
            }
            //Roles
            if (!context.Rol.Any())
            {             
                List<Rol> list = new List<Rol>();
                Rol item = new Rol() { Nombre = "Admin"};
                list.Add(item);
                item = new Rol() { Nombre = "User" };
                list.Add(item);
                context.Rol.AddRange(list);
                context.SaveChanges();                
            }
            if (!context.Usuario.Any())
            {
                //Crear Password
                string password = "activasol";
                string salt = SecurityHelper.CreateSalt(10);
                string passwordHash = SecurityHelper.CreatePasswordHash(password, salt);
                var roles = context.Rol.ToList();                                
                Usuario tmpUsuario = new Usuario() {  Nombre = "admin", Password = passwordHash, Salt = salt, Email = "pmassimino@hotmail.com", Estado = "ACTIVO"};
                tmpUsuario.Roles.Add(new UsuarioRol { IdUsuario = 1, IdRol = 1 });
                tmpUsuario.Roles.Add(new UsuarioRol { IdUsuario = 1, IdRol = 2 });
                context.Usuario.AddRange(tmpUsuario);
                context.SaveChanges();                
            }
        }
    }
}
