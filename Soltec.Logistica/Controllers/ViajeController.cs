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
    public class ViajeController : ControllerBase
    {

        ICommonService commonService;

        private IGenericRepository<Viaje> repository = null;
        private IRegistroRepository registroRepository = null;
        private IGenericRepository<TipoViaje> tipoViajeRepository = null;
        private IGenericRepository<EstadoRegistro> estadoRegistroRepository = null;
        private IGenericRepository<Chofer> choferRepository = null;        
        private IGenericRepository<MovPunto> movPuntoRepository = null;
        private ISessionService sessionService;          
        private readonly IMemoryCache cache;
        IConfiguration configuration;
        private IWebHostEnvironment hostingEnvironment;


        public ViajeController(ICommonService commonService, IConfiguration configuration,ISessionService sessionService, IMemoryCache memoryCache, IWebHostEnvironment environment)
        {   
            this.commonService = commonService;
            this.configuration = configuration;
            this.commonService.baseUrl = configuration["Soltec.Sae.Api:UrlService"].ToString();
            this.commonService.ApiKey = configuration["Soltec.Sae.Api:ApiKey"].ToString();            
            this.repository = new GenericRepository<Viaje>();
            this.registroRepository = new RegistroRepository();            
            this.choferRepository = new GenericRepository<Chofer>();                        
            this.repository = new GenericRepository<Viaje>();
            this.movPuntoRepository = new GenericRepository<MovPunto>(this.repository.context);
            this.tipoViajeRepository = new GenericRepository<TipoViaje>();
            this.estadoRegistroRepository = new GenericRepository<EstadoRegistro>();
            this.sessionService = sessionService;
            this.hostingEnvironment = environment;
            this.cache = memoryCache;
            this.registroRepository.context = repository.context;
            
            
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Add([FromBody] ViajeDTO entity)
        {
            var newEntity = new Viaje
            {
                Fecha = entity.Fecha,
                IdChofer = entity.IdChofer,
                IdTipo = entity.IdTipo,
                Concepto = entity.Concepto,
                NumeroComprobante = entity.NumeroComprobante,
                Distancia = entity.Distancia,
                Estado = "EnViaje",// Aquí puedes asignar el estado como desees
                IdTransaccion = Guid.NewGuid(),
            };
            var errorValidacion = this.Validate(newEntity);
            var registro = registroRepository.GetAll().Where(w=>w.IdChofer== entity.IdChofer).Include(i=>i.Estados).FirstOrDefault();
            if (registro != null)
            {
                string estado = registro.Estados.LastOrDefault().Estado;
                if (estado != "Disponible") 
                {
                    errorValidacion.Add(new string[] { "Registro", "Estado de registro no válido" });
                }
            }
            if (errorValidacion.Count > 0)
            {
                return BadRequest(errorValidacion);
            }            
            repository.Insert(newEntity);
            //Cambiar Estado
            int newid = estadoRegistroRepository.GetAll().Where(w => w.IdRegistro == registro.Id).Max(m => m.Id) + 1;
            EstadoRegistro estadoRegistro = new EstadoRegistro();
            estadoRegistro.Id = newid;
            estadoRegistro.Estado = "EnViaje";
            estadoRegistro.Registro = registro;
            estadoRegistro.IdRegistro = registro.Id;
            estadoRegistro.Fecha = DateTime.Now;
            registro.Estados.Add(estadoRegistro);            
            registroRepository.InsertEstadoRegistro(estadoRegistro);            
            repository.Save();            
            return Ok(entity);
        }
        [Route("{Id}")]
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int Id,[FromBody] ViajeDTO entity)
        {
            var newEntity = new Viaje
            {
                Fecha = entity.Fecha,
                IdChofer = entity.IdChofer,
                IdTipo = entity.IdTipo,
                Concepto = entity.Concepto,
                NumeroComprobante = entity.NumeroComprobante,
                Distancia = entity.Distancia,
                Estado = entity.Estado,                
            };
            var errorValidacion = this.Validate(newEntity);
            var currentViaje = repository.GetAll().Where(w => w.Id == Id).FirstOrDefault();
            if (currentViaje == null) 
            {
                errorValidacion.Add(new string[] { "Viaje", "No existe viaje con esos parametros" });
            }
            if (currentViaje != null)
            {                
                if (currentViaje.Estado == "Finalizado")
                {
                    errorValidacion.Add(new string[] { "Viaje", "Viaje Finalizado no se puede modificar" });
                }
            }
            if (errorValidacion.Count > 0)
            {
                return BadRequest(errorValidacion);
            }
            currentViaje.Fecha = entity.Fecha;
            currentViaje.IdChofer = entity.IdChofer;
            currentViaje.IdTipo = entity.IdTipo;
            currentViaje.Concepto = entity.Concepto;
            currentViaje.NumeroComprobante = entity.NumeroComprobante;
            currentViaje.Distancia = entity.Distancia;
            currentViaje.Estado = entity.Estado;
            
            repository.Update(currentViaje);
            if (newEntity.Estado == "Finalizado") 
            {
                var registro = registroRepository.GetAll().Where(w => w.IdChofer == entity.IdChofer).FirstOrDefault();
                //Cambiar Estado
                int newid = estadoRegistroRepository.GetAll().Where(w => w.IdRegistro == registro.Id).Max(m => m.Id) + 1;
                EstadoRegistro estadoRegistro = new EstadoRegistro();
                estadoRegistro.Id = newid;
                estadoRegistro.Estado = "Disponible";
                estadoRegistro.Registro = registro;
                estadoRegistro.IdRegistro = registro.Id;
                estadoRegistro.Fecha = DateTime.Now;
                registro.Estados.Add(estadoRegistro);
                registroRepository.InsertEstadoRegistro(estadoRegistro);
                //Registrar puntos
                MovPunto movPunto = new MovPunto();
                movPunto.Fecha = DateTime.Now;
                movPunto.IdChofer = entity.IdChofer;
                string concepto = "Viaje de Corta";
                double cantidad = tipoViajeRepository.GetById(entity.IdTipo).Puntos;
                if (entity.IdTipo == 2)
                {
                    concepto = "Viaje de Larga";
                }
                movPunto.Concepto = concepto;
                movPunto.Cantidad = cantidad;
                movPunto.IdTransaccion = currentViaje.IdTransaccion;
                movPunto.NumeroComprobante = entity.NumeroComprobante;
                movPuntoRepository.Insert(movPunto);
            }            
            repository.Save();
            

            return Ok(entity);
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
            var model = repository.GetAll().Include(i=>i.Chofer);
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

        private List<string[]> Validate(Viaje entity) 
        {
            List<string[]> errorValidacion = new List<string[]>();
            var chofer = choferRepository.GetById(entity.IdChofer);
            if (chofer == null)
            {
                errorValidacion.Add(new string[] { "Chofer", "Ingrese un chofer válido" });
            }            
            if (!Constants.EstadoViaje.Contains(entity.Estado))
            {
                errorValidacion.Add(new string[] { "Estado", "Estado no válido" });
            }
            var registro = registroRepository.GetAll().Where(w=>w.IdChofer==entity.IdChofer).Include(i=>i.Estados).FirstOrDefault();
            if (registro == null) 
            {
                errorValidacion.Add(new string[] { "Registro", "El Chofer no tiene un registro válido" });
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
