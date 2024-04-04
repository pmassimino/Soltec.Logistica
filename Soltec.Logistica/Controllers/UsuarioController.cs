using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soltec.Logistica.Service;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Soltec.Logistica.Code;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Soltec.Logistica.Model;

namespace Soltec.Logistica.Controllers
{

    public class UsuarioController : Controller
    {
        private IGenericRepository<Usuario> repository = null;
        private IGenericRepository<Rol> repositoryRol = null;
        private IGenericRepository<TicketValidacion> repositoryTicket = null;        
        private IConfiguration config;
        
        private ISessionService sessionService;

        public UsuarioController(IConfiguration config, IConfiguration configuration,ISessionService sessionService)
        {
            this.repository = new GenericRepository<Usuario>();
            this.repositoryTicket = new GenericRepository<TicketValidacion>();
            this.repositoryRol = new GenericRepository<Rol>();           
            this.config = config;
            this.sessionService = sessionService;
        }
        [HttpPost]
        [Route("api/login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] FormUsuario form)
        {
            List<string[]> errorValidacion = new List<string[]>();
            var result = this.ValidarLogin(form.Nombre, form.Password);
            if (result == false)
            {
                errorValidacion.Add(new string[] { "usuario", "usuario o contraseña incorrecto" });               
                var json = JsonConvert.SerializeObject(errorValidacion);
                //response.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                //return response;
                return Unauthorized(json);
            }
            var tmpUsuario = this.GetByName(form.Nombre);
            return Ok(new { token = GenerarTokenJWT(tmpUsuario) });
        }

        // GENERAMOS EL TOKEN CON LA INFORMACIÓN DEL USUARIO
        private string GenerarTokenJWT(UsuarioDto user)
        {
            var tmpclaims = new List<Claim>() {
                    new Claim(JwtRegisteredClaimNames.Sub, config["JWT:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                    new Claim("idUsuario", user.Id.ToString()),
                    new Claim("name", user.Nombre),
                    new Claim("email", user.Email)                    
                   };
            foreach (var role in user.Roles)
            {
                string nombre = repositoryRol.GetById(role.IdRol).Nombre;
                tmpclaims.Add(new  Claim(ClaimTypes.Role, nombre));
                tmpclaims.Add(new Claim("role", nombre));
            }
            var claims = tmpclaims.ToArray();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));

            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(config["Jwt:Issuer"], config["Jwt:Audience"], claims, expires: DateTime.UtcNow.AddDays(1), signingCredentials: signIn);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        [HttpPost]
        [Route("api/[controller]")]
        public IActionResult Add([FromBody] FormUsuario form)
        {
            List<string[]> errorValidacion = new List<string[]>();
            if (string.IsNullOrEmpty(form.Nombre))
            {
                errorValidacion.Add(new string[]{ "Nombre", "Ingrese un nombre válido"});
            }
            if (string.IsNullOrEmpty(form.email))
            {
                errorValidacion.Add(new string[] { "Nombre", "Ingrese un nombre válido" });
            }
            if (string.IsNullOrEmpty(form.Password))
            {
                errorValidacion.Add(new string[] { "Nombre", "Ingrese un nombre válido" });
            }
            //Validar nombre
            var tmpResult = this.GetByName(form.Nombre);
            if (tmpResult != null) 
            {               
                errorValidacion.Add(new string[] { "Nombre", "Usuario ya registrado" });
            }
            //Valiodar email
            var existeMail = repository.GetAll().Where(w=>w.Email.ToLower().Trim()==form.email.ToLower().Trim()).ToList().Count > 0;
            if (existeMail)
            {
                errorValidacion.Add(new string[] { "email", "email ya registrado" });
            }
            if (errorValidacion.Count > 0)
            {
                return BadRequest(errorValidacion);
            }
            //Create new
            Usuario newItem = new Usuario();
            newItem.Nombre = form.Nombre;
            newItem.Email = form.email;
            string salt = SecurityHelper.CreateSalt(10);
            newItem.Salt = salt;
            newItem.Password = SecurityHelper.CreatePasswordHash(form.Password,salt);
            //Rol Usuario            
            repository.Insert(newItem);
            repository.Save();
            newItem.Roles.Add(new UsuarioRol { IdUsuario = newItem.Id, IdRol = 2 });
            repository.Save();

            //Generar ticket de validación
            TicketValidacion ticket = new TicketValidacion();
            ticket.IdUsuario = newItem.Id;
            ticket.Tipo = "register";
            repositoryTicket.Insert(ticket);
            repositoryTicket.Save();            
            //Enviar email
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();
            System.Net.Mail.MailMessage correo = new System.Net.Mail.MailMessage();

            System.Net.Mail.Attachment adjunto;
            {
                var withBlock = smtp;
                withBlock.Port = Convert.ToInt32(config.GetValue<string>("emailSettings:smtpport"));

                withBlock.Host = config.GetValue<string>("emailSettings:smtpServer");
                withBlock.Credentials = new System.Net.NetworkCredential(config.GetValue<string>("emailSettings:smtpUser"), config.GetValue<string>("emailSettings:smtpPass"));
            }            
            {
                var withBlock = correo;
                withBlock.From = new System.Net.Mail.MailAddress(config.GetValue<string>("emailSettings:smtpMailFrom"));
                withBlock.To.Add(newItem.Email);
                withBlock.Subject = "Confirme su email";
                var sb = new StringBuilder();
                sb.Append("Hola, queremos verificar que usted es " + newItem.Nombre + ". Si ese es el caso, por favor siga el siguiente enlace:").AppendLine();
                var baseUri = $"{Request.Scheme}://{Request.Host}";

                string surl = baseUri + "/api/usuario/validateMail/" + ticket.Id; // url.Action("ValidateMail", "security", new { id = ticket.Id });
                
                string accion = " activar ";
                sb.Append("<a href=" + surl + ">" + "Click aqui para " + accion + "</a>").AppendLine();
                sb.Append("Si usted no es " + newItem.Nombre + " o no solicitó la verificación puede ignorar este mensaje.");
                string body = sb.ToString();
                withBlock.Body = body;
                withBlock.IsBodyHtml = true;
                withBlock.Priority = System.Net.Mail.MailPriority.Normal;
            }
            try
            {
                smtp.Send(correo);
            }
            catch (Exception ex)
            {
            }
            return Ok(form);
        }
        [Route("api/[controller]/{id}")]
        [Authorize(Roles = "Admin")]
        [HttpPut]        
        public IActionResult Edit(int id, [FromBody] Usuario item)
        {
            List<string[]> errorValidacion = new List<string[]>();
            
            var entity = repository.GetAll().Where(w => w.Id == item.Id).FirstOrDefault();
            if (entity == null)
            {
                errorValidacion.Add(new string[] { "Usuario", "No existe un usuario con esos parámetros" });
            }         
            if (new string[] { "ACTIVO", "PENDIENTE"}.Contains(item.Estado) == false)
            {
                errorValidacion.Add(new string[] { "Estado", "Estado no válido" });
            }
            //Validar nombre
            var tmpResult = this.GetByName(item.Nombre);
            if (tmpResult != null && tmpResult.Id != id)
            {
                errorValidacion.Add(new string[] { "Nombre", "Usuario ya registrado" });
            }
            //Validar email
            var existeMail = repository.GetAll().Where(w => w.Email.ToLower().Trim() == item.Email.ToLower().Trim() && w.Id != id).ToList().Count > 0;
            if (existeMail)
            {
                errorValidacion.Add(new string[] { "email", "email ya registrado" });
            }
            if (errorValidacion.Count > 0)
            {
                return BadRequest(errorValidacion);
            }
            entity.Nombre = item.Nombre;
            entity.Email = item.Email;
            entity.Estado = item.Estado;
            repository.Update(entity);
            repository.Save();
            return Ok();
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete()]        
        [Route("api/[controller]/{id}")]
        public IActionResult Delete(int id) 
        {
            //Dictionary<string, string> errorValidacion = new Dictionary<string, string>();
            List<string[]> errorValidacion = new List<string[]>();            
            var tmpUsuario = this.repository.GetAll().Where(w=>w.Id==id).FirstOrDefault();
            if (tmpUsuario == null) 
            {
                return NotFound();
            }

            if (tmpUsuario != null)
            {                
            }
            //Verificar si no es el actual usuario
            if (id == this.sessionService.IdUsuario) 
            {
                errorValidacion.Add(new string[] { "Id", "No puede borrar el usuario  su propio usuario" });
            }

            if (errorValidacion.Count > 0)
            {
                return BadRequest(errorValidacion);
            }
            this.repository.Delete(tmpUsuario.Id);
            this.repository.Save();
            return Ok();
        }
        [HttpGet()]
        [Route("api/[controller]/ValidateMail/{id}")]
        public IActionResult ValidateMail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var tmpTicket = this.repositoryTicket.GetById(id);
            if (tmpTicket == null)
            {
                return NotFound();
            }
            if (tmpTicket.FechaVencimiento < DateTime.Now) 
            {
                return BadRequest("Ticket Vencido");
            }
            if (tmpTicket.Estado == "FINALIZADO")
            {
                return BadRequest("Ticket no válido");
            }
            var tmpUsuario = this.repository.GetById(tmpTicket.IdUsuario);
            if (tmpUsuario == null) 
            {
                return BadRequest("Usuario no encontrado");
            }
            tmpUsuario.Estado = "ACTIVO";
            this.repository.Update(tmpUsuario);
            this.repository.Save();
            tmpTicket.Estado = "FINALIZADO";
            this.repositoryTicket.Update(tmpTicket);
            this.repositoryTicket.Save();            
            return Ok("Usuario Activado");
        }
        public  bool ValidarLogin(string usuario, string password)
        {
            string salt;
            string PasswordReal;
            bool result = false;
            UsuarioDto returnUser = null;
            var tmpUsuario = this.repository.GetAll().Where(w => w.Nombre == usuario).FirstOrDefault();

            if (tmpUsuario != null)
            {
                salt = tmpUsuario.Salt.Trim();
                PasswordReal = tmpUsuario.Password.Trim();
                string PasswordComp = SecurityHelper.CreatePasswordHash(password, salt);
                if ((PasswordReal ?? "") == (PasswordComp ?? ""))
                {
                    result = true;
                }
                // Chequear estado
                var estado = tmpUsuario.Estado;
                if (estado != "ACTIVO")
                {
                    result = false;
                }
            }
            return result;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet()]
        [Route("api/[controller]")]
        public IActionResult List() 
        {
            var result = repository.GetAll().Include(i=>i.Roles).Select(s => new UsuarioDto { Id = s.Id, Nombre = s.Nombre, Email = s.Email,Estado = s.Estado, Roles = s.Roles.ToList() });
            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet()]
        [Route("api/[controller]/{id}")]
        public IActionResult GetOne(int id)
        {
            var result = repository.GetAll().Where(w=>w.Id==id).Include(i => i.Roles).Select(s => new UsuarioDto { Id = s.Id, Nombre = s.Nombre, Email = s.Email, Estado = s.Estado, Roles = s.Roles.ToList()}).FirstOrDefault();
            return Ok(result);
        }
        
        public bool IsAdmin(decimal idUsuario)
        {
            bool result = false;           
            return result;
        }
       
        public UsuarioDto GetByName(string name)
        {
            var result = this.repository.GetAll().Include(i=>i.Roles).Where(w => w.Nombre.ToLower() == name.ToLower()).Select(s => new UsuarioDto { Id = s.Id, Nombre = s.Nombre, Email = s.Email,Roles=s.Roles.ToList() }).FirstOrDefault();
            return result;
        }
        public UsuarioDto GetByEmail(string email)
        {
            var result = this.repository.GetAll().Include(i => i.Roles).Where(w => w.Email.ToLower() == email.ToLower()).Select(s => new UsuarioDto { Id = s.Id, Nombre = s.Nombre, Email = s.Email, Roles = s.Roles.ToList() }).FirstOrDefault();
            return result;
            
        }

    }
    public class FormUsuario 
    {
        public string Nombre { get; set; }
        public string email { get; set; }
        public string Password { get;set;}
    }
    public class UsuarioDto
    {
        public UsuarioDto()
         {
            this.Roles = new List<UsuarioRol>();
         }
        public int Id { get; set; }
        public string Nombre { get; set; }     
        public string Email { get; set; }
        public string Estado { get; set; }
        public List<UsuarioRol> Roles { get;set; }        
    }
}

