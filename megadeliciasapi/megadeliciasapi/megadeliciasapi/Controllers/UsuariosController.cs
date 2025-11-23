using megadeliciasapi.Data;
using megadeliciasapi.Models;
using megadeliciasapi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BCrypt.Net;

namespace megadeliciasapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Verificar si el usuario actual es admin
        private bool IsAdmin()
        {
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            return rol?.ToLower() == "admin";
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
        {
            if (!IsAdmin())
            {
                return Forbid("Solo los administradores pueden ver la lista de usuarios.");
            }

            var usuarios = await _context.Usuarios
                .Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Correo = u.Correo,
                    Rol = u.Rol,
                    CreadoEn = u.CreadoEn,
                    RequiereCambioPassword = u.RequiereCambioPassword
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioDto>> GetUsuario(int id)
        {
            if (!IsAdmin())
            {
                return Forbid("Solo los administradores pueden ver usuarios.");
            }

            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            var usuarioDto = new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Rol = usuario.Rol,
                CreadoEn = usuario.CreadoEn,
                RequiereCambioPassword = usuario.RequiereCambioPassword
            };

            return Ok(usuarioDto);
        }

        // POST: api/Usuarios
        [HttpPost]
        public async Task<ActionResult<UsuarioDto>> CrearUsuario(CrearUsuarioDto dto)
        {
            if (!IsAdmin())
            {
                return Forbid("Solo los administradores pueden crear usuarios.");
            }

            // Verificar si el correo ya existe
            var correoExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == dto.Correo);

            if (correoExistente != null)
            {
                return BadRequest(new { message = "El correo ya está registrado." });
            }

            // Validar rol
            var rolesValidos = new[] { "admin", "mesero", "cocinero", "cajero", "contable" };
            if (!rolesValidos.Contains(dto.Rol.ToLower()))
            {
                return BadRequest(new { message = "El rol especificado no es válido." });
            }

            var nuevoUsuario = new Usuario
            {
                Nombre = dto.Nombre,
                Correo = dto.Correo,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Rol = dto.Rol,
                CreadoEn = DateTime.Now,
                RequiereCambioPassword = false
            };

            await _context.Usuarios.AddAsync(nuevoUsuario);
            await _context.SaveChangesAsync();

            var usuarioDto = new UsuarioDto
            {
                Id = nuevoUsuario.Id,
                Nombre = nuevoUsuario.Nombre,
                Correo = nuevoUsuario.Correo,
                Rol = nuevoUsuario.Rol,
                CreadoEn = nuevoUsuario.CreadoEn,
                RequiereCambioPassword = nuevoUsuario.RequiereCambioPassword
            };

            return CreatedAtAction(nameof(GetUsuario), new { id = nuevoUsuario.Id }, usuarioDto);
        }

        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarUsuario(int id, ActualizarUsuarioDto dto)
        {
            if (!IsAdmin())
            {
                return Forbid("Solo los administradores pueden actualizar usuarios.");
            }

            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            // Verificar si el correo ya existe en otro usuario
            var correoExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == dto.Correo && u.Id != id);

            if (correoExistente != null)
            {
                return BadRequest(new { message = "El correo ya está registrado por otro usuario." });
            }

            // Validar rol
            var rolesValidos = new[] { "admin", "mesero", "cocinero", "cajero", "contable" };
            if (!rolesValidos.Contains(dto.Rol.ToLower()))
            {
                return BadRequest(new { message = "El rol especificado no es válido." });
            }

            usuario.Nombre = dto.Nombre;
            usuario.Correo = dto.Correo;
            usuario.Rol = dto.Rol;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario actualizado exitosamente." });
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            if (!IsAdmin())
            {
                return Forbid("Solo los administradores pueden eliminar usuarios.");
            }

            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            // No permitir eliminar al propio usuario
            var userIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == usuario.Id.ToString())
            {
                return BadRequest(new { message = "No puedes eliminar tu propio usuario." });
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario eliminado exitosamente." });
        }

        // POST: api/Usuarios/5/cambiar-password
        [HttpPost("{id}/cambiar-password")]
        public async Task<IActionResult> CambiarPasswordUsuario(int id, CambiarPasswordUsuarioDto dto)
        {
            if (!IsAdmin())
            {
                return Forbid("Solo los administradores pueden cambiar contraseñas de usuarios.");
            }

            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NuevaPassword);
            usuario.RequiereCambioPassword = false;
            usuario.PasswordTemporalExpira = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Contraseña actualizada exitosamente." });
        }
    }
}
