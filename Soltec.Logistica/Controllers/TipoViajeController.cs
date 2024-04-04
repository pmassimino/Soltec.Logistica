using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soltec.Logistica.Code.BackEnd.Services.Core;
using Soltec.Logistica.Model;
using Soltec.Logistica.Service;

namespace Soltec.Logistica.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class TipoViajeController : Controller
    {
        private IGenericRepository<TipoViaje> repository = null;
        private IGenericRepository<Viaje> repositoryViaje = null;

        public TipoViajeController()
        {
            this.repository = new GenericRepository<TipoViaje>();            
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
        public ActionResult<TipoViaje> GetById(int Id)
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
        public IActionResult Add([FromBody] TipoViaje item)
        {
            List<string[]> errorValidacion = new List<string[]>();
            if (string.IsNullOrEmpty(item.Nombre))
            {
                errorValidacion.Add(new string[] { "Nombre", "Ingrese un nombre válido" });
            }
            if (errorValidacion.Count > 0)
            {
                return BadRequest(errorValidacion);
            }
            repository.Insert(item);
            repository.Save();
            return Ok(item);
        }
        [Route("{Id}")]
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id,[FromBody] TipoViaje item)
        {
            List<string[]> errorValidacion = new List<string[]>(); ;
            var entity = repository.GetAll().Where(w => w.Id == item.Id).FirstOrDefault();
            if (entity == null)
            {
                errorValidacion.Add(new string[] { "TipoViaje", "No existe un tipo de viaje con esos parámetros" });
            }            

            if (string.IsNullOrEmpty(item.Nombre))
            {
                errorValidacion.Add(new string[] { "Nombre", "Ingrese un nombre válido" });
            }            
            
            if (errorValidacion.Count > 0)
            {
                return BadRequest(errorValidacion);
            }
            entity.Nombre = item.Nombre;            
            entity.Puntos = item.Puntos;
            repository.Update(entity);         
            repository.Save();
            return Ok(item);
        }
        [HttpDelete]
        [Route("{Id}")]
        [Authorize(Roles = "Admin")]
        public ActionResult<TipoViaje> Delete(int Id)
        {
            List<string[]> errorValidacion = new List<string[]>();
            var result = repository.GetById(Id);
            if (result == null)
            {
                errorValidacion.Add(new string[] { "TipoViaje", "Tipo Viaje no existe" });
            }
            if (result != null)
            {
                var tieneReg = this.repositoryViaje.GetAll().Where(w => w.IdTipo == Id).Count();
                if (tieneReg > 0)
                {
                    errorValidacion.Add(new string[] { "TipoViaje", "El tipo de viaje tiene registros , no se puede eliminar" });
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


        }
    }

