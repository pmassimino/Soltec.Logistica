using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soltec.Logistica.Service;
using Soltec.Logistica.Model;
using Soltec.Logistica.Code;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Soltec.Logistica.Data;
using static iTextSharp.text.pdf.AcroFields;
using Microsoft.EntityFrameworkCore.Internal;
using SQLitePCL;

namespace Soltec.Logistica.Controllers
{
    [ApiController]  
    [Route("api/[controller]")]
    public class RegistroController : ControllerBase
    {

        ICommonService commonService;        
        
        private IRegistroRepository repository = null;
        private IGenericRepository<EstadoRegistro> estadoRegistrorepository = null;
        private IGenericRepository<Usuario> usuarioRepository;
        private IGenericRepository<Chofer> choferRepository = null;
        private IGenericRepository<Viaje> viajeRepository = null;
        private IGenericRepository<MovPunto> movPuntoRepository = null;
        private ISessionService sessionService;          
        private readonly IMemoryCache cache;
        IConfiguration configuration;
        private IWebHostEnvironment hostingEnvironment;


        public RegistroController(ICommonService commonService, IConfiguration configuration,ISessionService sessionService, IMemoryCache memoryCache, IWebHostEnvironment environment)
        {   
            this.commonService = commonService;
            this.configuration = configuration;
            this.commonService.baseUrl = configuration["Soltec.Sae.Api:UrlService"].ToString();
            this.commonService.ApiKey = configuration["Soltec.Sae.Api:ApiKey"].ToString();          
            this.repository = new RegistroRepository();
            this.estadoRegistrorepository = new GenericRepository<EstadoRegistro>();
            this.choferRepository = new GenericRepository<Chofer>();
            this.usuarioRepository = new GenericRepository<Usuario>();
            this.movPuntoRepository = new GenericRepository<MovPunto>();
            this.viajeRepository = new GenericRepository<Viaje>();
            this.sessionService = sessionService;
            this.hostingEnvironment = environment;
            this.cache = memoryCache;
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Add([FromBody] RegistroDTO entity)
        {
            var errorValidacion = this.Validate(entity);
            var registro = repository.GetAll().Where(w=>w.IdChofer== entity.IdChofer).FirstOrDefault();
            if (registro != null) 
            {
                errorValidacion.Add(new string[] { "Registro", "El Chofer ya se encuentra registrado" });
            }
            if (errorValidacion.Count > 0)
            {
                return BadRequest(errorValidacion);
            }
            Registro newEntity = new Registro();
            //newEntity.Id = Guid.NewGuid();  
            newEntity.IdChofer = entity.IdChofer;
            newEntity.Fecha = entity.Fecha;            
            EstadoRegistro estadoRegistro = new EstadoRegistro();
            //estadoRegistro.Id =  Guid.NewGuid();
            estadoRegistro.Id = 1;
            estadoRegistro.IdRegistro = newEntity.Id;
            estadoRegistro.Fecha = DateTime.Now;            
            estadoRegistro.Estado = entity.Estado;
            estadoRegistro.IdRegistro = newEntity.Id;
            estadoRegistro.Registro = newEntity;
            newEntity.Estados.Add(estadoRegistro);
            repository.Insert(newEntity);
            repository.Save();
            return Ok(newEntity);
        }
        [Route("{Id}")]
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateEstado(int id,[FromBody] RegistroDTO entity)
        {
            var errorValidacion = Validate(entity);
            var registro = repository.GetAll().Where(w=>w.Id ==id).Include(i=>i.Estados).FirstOrDefault();
            if (registro == null)
            {
                errorValidacion.Add(new string[] { "Registro", "Registro no existe" });
            }                        
            if (registro != null) 
            {
                if (registro.Estados.LastOrDefault().Estado=="EnViaje") 
                {
                    errorValidacion.Add(new string[] { "Estado", "debe Finalizar el Viaje para cambiar el estado" });
                }
                if (registro.Estados.LastOrDefault().Estado == entity.Estado) 
                {
                    errorValidacion.Add(new string[] { "Estado", "cambio de estado no valida" });
                }
            }
            if (errorValidacion.Count > 0)
            {
                return BadRequest(errorValidacion);
            }
            int newid = estadoRegistrorepository.GetAll().Where(w => w.IdRegistro == id).Max(m=>m.Id) + 1;
            EstadoRegistro estadoRegistro = new EstadoRegistro();
            estadoRegistro.Id = newid;
            estadoRegistro.IdRegistro = registro.Id;
            estadoRegistro.Fecha = DateTime.Now;
            estadoRegistro.Estado = entity.Estado;
            estadoRegistro.IdRegistro = registro.Id;
            estadoRegistro.Registro = registro;
            registro.Estados.Add(estadoRegistro);            
            repository.InsertEstadoRegistro(estadoRegistro);
            repository.Save();
            //estadoRegistrorepository.Insert(estadoRegistro);
            //estadoRegistrorepository.Save();
            return Ok(estadoRegistro);
        }
        [Authorize(Roles = "Admin")]
      
        [HttpDelete]
        [Route("{Id}")]
        [Authorize(Roles = "Admin")]
        public ActionResult<Registro> Delete(int Id)
        {
            List<string[]> errorValidacion = new List<string[]>();
            var result = repository.GetById(Id);
            if (result == null)
            {
                errorValidacion.Add(new string[] { "Registro", "Registro no existe" });
            }            
            if (errorValidacion.Count > 0)
            {
                return BadRequest(errorValidacion);
            }
            repository.Delete(Id);
            repository.Save();
            return Ok(result);
        }
        [HttpGet]
        [Route("")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAll()
        {
            var model = repository.GetAll().Include(i=>i.Chofer).Include(i=>i.Estados);
            return Ok(model);
        }
        
        [HttpGet]
        [Route("view")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllView()
        {
            var dbContext = new LogisticaContext();

            var result = dbContext.Registro
             .Select(registro => new 
             {
                 Id = registro.Id,
                 Fecha = registro.Estados.OrderBy(o => o.Fecha).LastOrDefault().Fecha,
                 IdChofer = registro.IdChofer,
                 NombreChofer = registro.Chofer.Nombre,
                 Patente = registro.Chofer.Patente,
                 PatenteAcoplado = registro.Chofer.PatenteAcoplado,
                 Estado = registro.Estados.OrderBy(o => o.Fecha).LastOrDefault().Estado,
                 CantidadCorto = dbContext.Viaje
                     .Where(viaje => viaje.IdChofer == registro.IdChofer && viaje.IdTipo == 1)
                     .Count(),
                 CantidadLargo = dbContext.Viaje
                     .Where(viaje => viaje.IdChofer == registro.IdChofer && viaje.IdTipo == 2)
                     .Count(),
                 Puntos = dbContext.MovPunto
                     .Where(movPunto => movPunto.IdChofer == registro.IdChofer)
                     .Sum(s => s.Cantidad)
             });
            

            return Ok(result);
        }
        [HttpGet]
        [Route("{Id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetById(int id)
        {
            var model = repository.GetById(id);
            return Ok(model);
        }

        private List<string[]> Validate(RegistroDTO entity) 
        {
            List<string[]> errorValidacion = new List<string[]>();
            var chofer = choferRepository.GetById(entity.IdChofer);
            if (chofer == null)
            {
                errorValidacion.Add(new string[] { "Chofer", "Ingrese un chofer válido" });
            }
            if (!Constants.EstadoRegistro.Contains(entity.Estado))
            {
                errorValidacion.Add(new string[] { "Estado", "Estado no válido" });
            }
            return errorValidacion;
        }
        public class EstadoLogistica 
        {
            public string Estado { get; set; } = "";
            public string Mensage { get; set; } = "";
        }


    }
}
