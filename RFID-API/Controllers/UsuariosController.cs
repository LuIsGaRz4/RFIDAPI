using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RFID_API.Data;

namespace RFID_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Usuarios/rol/Luis Garza
        [HttpGet("rol/{nombre}")]
        public async Task<ActionResult<string>> GetRolPorNombre(string nombre)
        {
            var usuario = await _context.RFID_USUARIOS.FirstOrDefaultAsync(u => u.Nombre == nombre);
            if (usuario == null)
                return NotFound("Usuario no encontrado");

            return Ok(usuario.Rol); // Devuelve "Supervisor" o "Empleado"
        }
    }

}
