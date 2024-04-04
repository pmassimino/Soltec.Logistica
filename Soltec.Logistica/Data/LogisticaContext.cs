using Microsoft.EntityFrameworkCore;
using Soltec.Logistica.Code;
using Soltec.Logistica.Model;
using System.Security.Cryptography;
using System.Text;

namespace Soltec.Logistica.Data
{
    public class LogisticaContext : DbContext
    {
        public DbSet<Usuario> Usuario { get; set; }        
        public DbSet<Rol> Rol { get; set; }
        public DbSet<TicketValidacion> TicketValidacion { get; set; }
        IConfiguration configuration;
        public DbSet<Chofer> Chofer { get; set; }
        public DbSet<Transporte> Transporte { get; set; }
        public DbSet<Registro> Registro { get; set; }
        public DbSet<Viaje> Viaje { get; set; }
        public DbSet<TipoViaje> TipoViaje { get; set; }
        public DbSet<MovPunto> MovPunto { get; set; }


        public string DbPath { get; }

        public LogisticaContext()
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            this.configuration = configurationBuilder.Build();
            this.DbPath = configuration["DatabasePath"] + "\\DataBase.db";
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        //protected override void OnConfiguring(DbContextOptionsBuilder options)
        //    => options.UseSqlite($"Data Source={DbPath}");
        // }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
            //=> options.UseSqlServer(this.configuration.GetConnectionString("DefaultConnection"));
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<UsuarioRol>()
            .HasKey(ur => new { ur.IdUsuario, ur.IdRol });
            modelBuilder.Entity<UsuarioRol>()
                .HasOne(ur => ur.Usuario)
                .WithMany(u => u.Roles)
                .HasForeignKey(ur => ur.IdUsuario);
            modelBuilder.Entity<UsuarioRol>()
                .HasOne(ur => ur.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(ur => ur.IdRol)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TransporteChofer>()
                .HasKey(tc => new { tc.IdTransporte, tc.IdChofer });
            modelBuilder.Entity<TransporteChofer>()                
                .HasOne(ur => ur.Transporte)
                .WithMany(r => r.Choferes)
                .HasForeignKey(ur => ur.IdChofer)
                .OnDelete(DeleteBehavior.Cascade);
            //EstadoRegistro            
            modelBuilder.Entity<EstadoRegistro>()
           .HasKey(ur => new { ur.Id, ur.IdRegistro });
            modelBuilder.Entity<EstadoRegistro>()
                .HasOne(ur => ur.Registro)
                .WithMany(r => r.Estados)
                .HasForeignKey(ur => ur.IdRegistro)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EstadoRegistro>()
                .HasOne(ur => ur.Registro)
                .WithMany(r => r.Estados)
                .HasForeignKey(ur => ur.IdRegistro)
                .OnDelete(DeleteBehavior.Cascade);
              
            

            //Clientes
            //modelBuilder.Entity<UsuarioCuenta>()
            //.HasKey(ur => new { ur.IdUsuario, ur.IdCuenta });
            //modelBuilder.Entity<UsuarioCuenta>()
            //    .HasOne(ur => ur.Usuario)
            //    .WithMany(u => u.Cuentas)
            //    .HasForeignKey(ur => ur.IdUsuario);



        }



    }
}