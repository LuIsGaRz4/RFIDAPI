using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RFID_API.Data;
using RFID_API.Models;

[ApiController]
[Route("api/[controller]")]
public class RFIDRegistroController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RFIDRegistroController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/RFIDRegistro
    [HttpGet("MostrarTodoRegistro")]
    public async Task<ActionResult<IEnumerable<RFIDRegistros>>> GetRegistros()
    {
        return await _context.RFID_REGISTRO.ToListAsync();
    }

    [HttpGet("validar-tarjeta/{idTarjeta}")]
    public async Task<IActionResult> ValidarTarjeta(string idTarjeta)
    {
        var tarjeta = await _context.RFID_TARJETAS.FirstOrDefaultAsync(t => t.IdTarjeta == idTarjeta);

        if (tarjeta == null)
            return NotFound("Tarjeta no válida");

        return Ok(tarjeta);
    }
    [HttpGet("rolPorTarjeta/{idTarjeta}")]
    public async Task<IActionResult> GetRolPorTarjeta(string idTarjeta)
    {
        var usuario = await _context.RFID_USUARIOS
            .FirstOrDefaultAsync(u => u.IdTarjeta == idTarjeta);

        if (usuario == null)
            return NotFound("Tarjeta no encontrada");

        return Ok(new { nombre = usuario.Nombre, rol = usuario.Rol });

    }
    [HttpGet("MostrarTodasLasTarjetas")]
    public async Task<ActionResult<IEnumerable<RFIDTarjetas>>> GetTarjetas()
    {
        return await _context.RFID_TARJETAS.ToListAsync();
    }


    [HttpGet("MostrarTarjetas")]
    public async Task<ActionResult<IEnumerable<RFIDUsuarios>>> GetUsuarios()
    {
        return await _context.RFID_USUARIOS.ToListAsync();
    }









    // POST: api/RFIDRegistro
    [HttpPost("AgregarRegistro")]
    public async Task<ActionResult<RFIDRegistros>> PostRegistro(RFIDRegistros registro)
    {
        // Buscar el nombre por IdRegistro en la tabla RFID_TARJETAS
        var tarjeta = await _context.RFID_TARJETAS.FindAsync(registro.IdRegistro);

        if (tarjeta == null)
            return BadRequest("Tarjeta no registrada");

        // Rellenar automáticamente el nombre
        registro.Nombre = tarjeta.Nombre;
        registro.Fecha = DateTime.UtcNow;

        _context.RFID_REGISTRO.Add(registro);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRegistros), new { id = registro.IdRegistro }, registro);
    }
    [HttpPost("AgregarTarjetas")]
    public async Task<IActionResult> AgregarTarjeta([FromBody] RFIDTarjetas nuevaTarjeta)
    {
        if (nuevaTarjeta == null || string.IsNullOrWhiteSpace(nuevaTarjeta.IdTarjeta))
            return BadRequest("Datos inválidos.");

        var existe = await _context.RFID_TARJETAS
            .AnyAsync(t => t.IdTarjeta == nuevaTarjeta.IdTarjeta);

        if (existe)
            return Conflict("Esta tarjeta ya existe.");

        _context.RFID_TARJETAS.Add(nuevaTarjeta);
        await _context.SaveChangesAsync();

        return Ok(nuevaTarjeta);
    }
    [HttpPost("Agregar")]
    public async Task<IActionResult> CreateUsuario([FromBody] RFIDUsuarios usuario)
    {
        if (usuario == null)
            return BadRequest("El usuario es nulo.");

        _context.RFID_USUARIOS.Add(usuario);
        await _context.SaveChangesAsync();
        return Ok(usuario);
    }

    //PUT: api/RFIDRegistro
    [HttpPut("Actualizarregistro")]
    public async Task<IActionResult> PutRegistro(
    [FromBody] RFIDRegistros registro,
    [FromHeader(Name = "idTarjeta")] string idTarjeta)
    {
        if (registro.IdRegistro == null)
            return BadRequest("El IdRegistro es obligatorio.");

        // Validar si el usuario que hace la petición es supervisor
        var usuario = await _context.RFID_USUARIOS
            .FirstOrDefaultAsync(u => u.IdTarjeta == idTarjeta);

        if (usuario == null)
            return Unauthorized("Usuario no encontrado.");

        if (usuario.Rol != "Supervisor")
            return Forbid("No tienes permiso para modificar registros.");

        var registroExistente = await _context.RFID_REGISTRO.FindAsync(registro.IdRegistro);
        if (registroExistente == null)
            return NotFound();

        registroExistente.IdAccesos = registro.IdAccesos;
        registroExistente.Nombre = registro.Nombre;
        registroExistente.Fecha = registro.Fecha;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.RFID_REGISTRO.Any(e => e.IdRegistro == registro.IdRegistro))
                return NotFound();
            else
                throw;
        }

        return Ok(registroExistente);
    }
    [HttpPut("ActualizarPorTarjeta/{idTarjeta}")]
    public async Task<IActionResult> UpdateUsuarioPorTarjeta(string idTarjeta, [FromBody] RFIDUsuarios usuarioActualizado)
    {
        var usuario = await _context.RFID_USUARIOS
            .FirstOrDefaultAsync(u => u.IdTarjeta == idTarjeta);

        if (usuario == null)
            return NotFound(new { mensaje = "Usuario no encontrado." });

        usuario.Nombre = usuarioActualizado.Nombre;
        usuario.Rol = usuarioActualizado.Rol;
        // usuario.IdTarjeta no se cambia porque es la clave para encontrarlo

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Usuario actualizado correctamente." });
    }



    // DELETE: api/RFIDRegistro/RFID-1001
    [HttpDelete("BorrarRegistro/{idRegistro}")]
    public async Task<IActionResult> DeleteRegistro(string idRegistro, [FromHeader(Name = "idTarjeta")] string idTarjeta)
    {
        var usuario = await _context.RFID_USUARIOS
            .FirstOrDefaultAsync(u => u.IdTarjeta == idTarjeta);

        if (usuario == null)
            return Unauthorized("Usuario no encontrado.");

        if (usuario.Rol != "Supervisor")
            return Forbid("No tienes permiso para eliminar registros.");

        var registro = await _context.RFID_REGISTRO.FindAsync(idRegistro);
        if (registro == null)
            return NotFound();

        _context.RFID_REGISTRO.Remove(registro);
        await _context.SaveChangesAsync();

        return Ok(registro);
    }
    [HttpDelete("BorrarTarjeta/{idTarjetaEliminar}")]
    public async Task<IActionResult> EliminarTarjeta(
    string idTarjetaEliminar,
    [FromHeader(Name = "idTarjeta")] string idTarjetaSolicitante)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(idTarjetaSolicitante))
                return Unauthorized("IdTarjeta solicitante no proporcionado.");

            var usuario = await _context.RFID_USUARIOS
                .FirstOrDefaultAsync(u => u.IdTarjeta == idTarjetaSolicitante);

            if (usuario == null)
                return Unauthorized("Usuario no encontrado.");

            if (usuario.Rol != "Supervisor")
                return Forbid("No tienes permiso para eliminar tarjetas.");

            if (string.IsNullOrWhiteSpace(idTarjetaEliminar))
                return BadRequest("ID de tarjeta inválido.");

            var tarjeta = await _context.RFID_TARJETAS.FindAsync(idTarjetaEliminar);

            if (tarjeta == null)
                return NotFound("Tarjeta no encontrada.");

            _context.RFID_TARJETAS.Remove(tarjeta);
            await _context.SaveChangesAsync();

            return Ok($"Tarjeta {idTarjetaEliminar} eliminada correctamente.");
        }
        catch (Exception ex)
        {
            // Aquí puedes registrar el error con logger o consola
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }
   
    [HttpDelete("EliminarPorTarjeta/{idTarjeta}")]
    public async Task<IActionResult> DeleteUsuarioPorTarjeta(string idTarjeta)
    {
        var usuario = await _context.RFID_USUARIOS
            .FirstOrDefaultAsync(u => u.IdTarjeta == idTarjeta);

        if (usuario == null)
            return NotFound(new { mensaje = "Usuario no encontrado con esa tarjeta." });

        _context.RFID_USUARIOS.Remove(usuario);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Usuario eliminado correctamente." });
    }






}
