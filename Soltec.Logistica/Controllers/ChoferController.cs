using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soltec.Logistica.Code.BackEnd.Services.Core;
using Soltec.Logistica.Model;
using Soltec.Logistica.Service;
using static iTextSharp.text.pdf.AcroFields;
using static iTextSharp.text.pdf.events.IndexEvents;

namespace Soltec.Logistica.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ChoferController : Controller
    {
        private IGenericRepository<Chofer> repository = null;
        private IGenericRepository<Registro> registroRepository = null;

        public ChoferController()
        {
            this.repository = new GenericRepository<Chofer>();
            this.registroRepository = new GenericRepository<Registro>();
        }
        [HttpGet]       
        [Route("")]
        public IActionResult GetAll()
        {
            var model = repository.GetAll();
            return Ok(model);
        }
        [HttpGet]
        [Route("{Id}")]
        public ActionResult<Chofer> GetById(int Id)
        {
            var result = repository.GetById(Id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Add([FromBody] Chofer entity)
        {
            var errorValidacion = this.Validate(entity);
            var existeCuit = repository.GetAll().Where(w=>w.NumeroDocumento==entity.NumeroDocumento).Count() > 0;
            if (existeCuit) 
            {
                errorValidacion.Add(new string[] { "NumeroDocumento", "Cuit ya registrado" });
            }
            if (errorValidacion.Count > 0)
            {
                return BadRequest(errorValidacion);
            }
            repository.Insert(entity);
            repository.Save();
            return Ok(entity);
        }
        [Route("{Id}")]
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id,[FromBody] Chofer entity)
        {
            var errorValidacion = this.Validate(entity);
            var entityCurrent = repository.GetById(id);
            if (entityCurrent==null)
            {
                errorValidacion.Add(new string[] { "Chofer", "Chofer no existe" });
            }
            var existeCuit = repository.GetAll().Where(w => w.NumeroDocumento == entity.NumeroDocumento && w.Id != id).Count() > 0;
            if (existeCuit)
            {
                errorValidacion.Add(new string[] { "NumeroDocumento", "Cuit ya registrado" });
            }
            if (errorValidacion.Count > 0)
            {
                return BadRequest(errorValidacion);
            }
            entityCurrent.Nombre = entity.Nombre;
            entityCurrent.Email = entity.Email;
            entityCurrent.Telefono = entity.Telefono;
            entityCurrent.NumeroDocumento = entity.NumeroDocumento;
            entityCurrent.Patente = entity.Patente;
            entityCurrent.PatenteAcoplado = entity.PatenteAcoplado;

            repository.Update(entityCurrent);         
            repository.Save();
            return Ok(entityCurrent);
        }
        [HttpDelete]
        [Route("{Id}")]
        [Authorize(Roles = "Admin")]
        public ActionResult<Chofer> Delete(int Id)
        {
            List<string[]> errorValidacion = new List<string[]>();
            var result = repository.GetById(Id);
            if (result == null)
            {
                errorValidacion.Add(new string[] { "Chofer", "Chofer no existe" });
            }
            if (result != null)
            {
                var tieneReg = this.registroRepository.GetAll().Where(w => w.IdChofer == Id).Count();
                if (tieneReg > 0)
                {
                    errorValidacion.Add(new string[] { "Chofer", "El chofer tiene registros , no se puede eliminar" });
                }
            }
                if (errorValidacion.Count > 0)
                {
                    return BadRequest(errorValidacion);
                }
                repository.Delete(Id);
                repository.Save();
                return Ok(result);
            }

        public List<string[]> Validate(Chofer entity) 
        {
            List<string[]> errorValidacion = new List<string[]>();
            if (string.IsNullOrEmpty(entity.Nombre))
            {
                errorValidacion.Add(new string[] { "Nombre", "Ingrese un nombre válido" });
            }
            if (CoreServices.ValidaCuit(entity.NumeroDocumento.ToString()) == false)
            {
                errorValidacion.Add(new string[] { "NumeroDocumento", "Ingrese un numero de cuit válido" });
            }
            if (!string.IsNullOrEmpty(entity.Email))
            {
                if (!CoreServices.IsValidEmail(entity.Email))
                {
                    errorValidacion.Add(new string[] { "email", "Ingrese un mail válido" });
                }
            }
            if (string.IsNullOrEmpty(entity.Patente))
            {
                errorValidacion.Add(new string[] { "Patente", "Ingrese una patente válida" });
            }
            return errorValidacion;
        }


        }
    }

