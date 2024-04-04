using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soltec.Logistica.Code.BackEnd.Services.Core;
using Soltec.Logistica.Model;
using Soltec.Logistica.Service;

namespace Soltec.Logistica.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class MovPuntoController : Controller
    {
        private IGenericRepository<MovPunto> repository = null;
        private IGenericRepository<Viaje> repositoryViaje = null;

        public MovPuntoController()
        {
            this.repository = new GenericRepository<MovPunto>();            
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
        

        }
    }

