using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR; // 👈 Añadido para SignalR
using RFID_API.Data;
using RFID_API.Models;
using RFID_API.Hubs;
using System.Globalization;


[ApiController]
[Route("api/[controller]")]
public class RFIDRegistroController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext; // 👈 Añadido para SignalR

    public RFIDRegistroController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

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
        var usuario = await _context.RFID_USUARIOS.FirstOrDefaultAsync(u => u.IdTarjeta == idTarjeta);
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

    [HttpPost("AgregarRegistro")]
    public async Task<ActionResult<RFIDRegistros>> PostRegistro(RFIDRegistros registro)
    {
        var tarjeta = await _context.RFID_TARJETAS.FindAsync(registro.IdRegistro);
        if (tarjeta == null)
            return BadRequest("Tarjeta no registrada");

        registro.Nombre = tarjeta.Nombre;

        // Asignar acceso aleatorio: true o false
        var random = new Random();
        registro.IdAccesos = random.Next(0, 2) == 1; // 0 o 1, si es 1 es true

        // Convertir la fecha UTC a hora local de Reynosa (Central Standard Time)
        var zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        registro.Fecha = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaHoraria);

        _context.RFID_REGISTRO.Add(registro);
        await _context.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("RecibirActualizacion", "registro-agregado");

        return CreatedAtAction(nameof(GetRegistros), new { id = registro.IdRegistro }, registro);
    }



    [HttpPost("AgregarTarjetas")]
    public async Task<IActionResult> AgregarTarjeta([FromBody] RFIDTarjetas nuevaTarjeta)
    {
        if (nuevaTarjeta == null || string.IsNullOrWhiteSpace(nuevaTarjeta.IdTarjeta))
            return BadRequest("Datos inválidos.");

        var existe = await _context.RFID_TARJETAS.AnyAsync(t => t.IdTarjeta == nuevaTarjeta.IdTarjeta);
        if (existe)
            return Conflict("Esta tarjeta ya existe.");

        _context.RFID_TARJETAS.Add(nuevaTarjeta);
        await _context.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("RecibirActualizacion", "tarjeta-agregada");

        return Ok(nuevaTarjeta);
    }

    [HttpPost("Agregar")]
    public async Task<IActionResult> CreateUsuario([FromBody] RFIDUsuarios usuario)
    {
        if (usuario == null)
            return BadRequest("El usuario es nulo.");

        _context.RFID_USUARIOS.Add(usuario);
        await _context.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("RecibirActualizacion", "usuario-agregado");

        return Ok(usuario);
    }

    [HttpPut("ActualizarRegistro/{id}")]
    public async Task<IActionResult> PutRegistro(int id, [FromBody] RFIDRegistros registroActualizado)
    {
        // Buscar el registro existente por ID (int)
        var registroExistente = await _context.RFID_REGISTRO.FirstOrDefaultAsync(r => r.Id == id);
        if (registroExistente == null)
            return NotFound($"No se encontró ningún registro con Id '{id}'.");

        // Actualizar los campos permitidos
        registroExistente.IdAccesos = registroActualizado.IdAccesos;
        registroExistente.Nombre = registroActualizado.Nombre;
        registroExistente.Fecha = registroActualizado.Fecha;

        // Guardar cambios
        await _context.SaveChangesAsync();

        // Notificación por SignalR
        await _hubContext.Clients.All.SendAsync("RecibirActualizacion", "registro-actualizado");

        return Ok(new
        {
            mensaje = "Registro actualizado correctamente",
            registro = new
            {
                registroExistente.Id,
                registroExistente.IdRegistro,
                registroExistente.Nombre,
                registroExistente.Fecha,
                registroExistente.IdAccesos
            }
        });
    }





    [HttpPut("ActualizarPorTarjeta/{idTarjeta}")]
    public async Task<IActionResult> UpdateUsuarioPorTarjeta(string idTarjeta, [FromBody] RFIDUsuarios usuarioActualizado)
    {
        var usuario = await _context.RFID_USUARIOS.FirstOrDefaultAsync(u => u.IdTarjeta == idTarjeta);
        if (usuario == null)
            return NotFound(new { mensaje = "Usuario no encontrado." });

        usuario.Nombre = usuarioActualizado.Nombre;
        usuario.Rol = usuarioActualizado.Rol;

        await _context.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("RecibirActualizacion", "usuario-actualizado");

        return Ok(new { mensaje = "Usuario actualizado correctamente." });
    }

    [HttpDelete("BorrarRegistro/{id}")]
    public async Task<IActionResult> DeleteRegistro(int id)
    {
        var registro = await _context.RFID_REGISTRO.FindAsync(id);

        if (registro == null)
            return NotFound($"No se encontró ningún registro con Id '{id}'.");

        _context.RFID_REGISTRO.Remove(registro);
        await _context.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("RecibirActualizacion", "registro-eliminado");

        return Ok(new
        {
            mensaje = $"Se eliminó el registro con Id '{id}'.",
            registroEliminado = new
            {
                registro.Id,
                registro.IdRegistro,
                registro.IdAccesos,
                registro.Nombre,
                registro.Fecha
            }
        });
    }







    [HttpDelete("BorrarTarjeta/{idTarjetaEliminar}")]
    public async Task<IActionResult> EliminarTarjeta(string idTarjetaEliminar, [FromHeader(Name = "idTarjeta")] string idTarjetaSolicitante)
    {
        if (string.IsNullOrWhiteSpace(idTarjetaSolicitante))
            return Unauthorized("IdTarjeta solicitante no proporcionado.");

        var usuario = await _context.RFID_USUARIOS.FirstOrDefaultAsync(u => u.IdTarjeta == idTarjetaSolicitante);
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
        await _hubContext.Clients.All.SendAsync("RecibirActualizacion", "tarjeta-eliminada");

        return Ok($"Tarjeta {idTarjetaEliminar} eliminada correctamente.");
    }

    [HttpDelete("EliminarPorTarjeta/{idTarjeta}")]
    public async Task<IActionResult> DeleteUsuarioPorTarjeta(string idTarjeta)
    {
        var usuario = await _context.RFID_USUARIOS.FirstOrDefaultAsync(u => u.IdTarjeta == idTarjeta);
        if (usuario == null)
            return NotFound(new { mensaje = "Usuario no encontrado con esa tarjeta." });

        _context.RFID_USUARIOS.Remove(usuario);
        await _context.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("RecibirActualizacion", "usuario-eliminado");

        return Ok(new { mensaje = "Usuario eliminado correctamente." });
    }
}