using megadeliciasapi.Data;
using megadeliciasapi.Models;
using megadeliciasapi.DTOs;
using megadeliciasapi.Services; // <-- Importante para usar el servicio de email
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net; 

namespace megadeliciasapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService; // <-- 1. Inyectamos el servicio

        // Constructor actualizado
        public AuthController(ApplicationDbContext context, IConfiguration config, IEmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService; // <-- Guardamos la referencia
        }

        // --- 1. REGISTRO (Igual que antes) ---
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            var usuarioExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == request.Correo);
            if (usuarioExistente != null) return BadRequest(new { message = "El correo ya está registrado." });

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var nuevoUsuario = new Usuario
            {
                Nombre = request.Nombre,
                Correo = request.Correo,
                PasswordHash = passwordHash,
                Rol = request.Rol,
                CreadoEn = DateTime.Now,
                RequiereCambioPassword = false // Al registrarse normal, no requiere cambio
            };

            await _context.Usuarios.AddAsync(nuevoUsuario);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { message = "Usuario creado exitosamente" });
        }

        // --- 2. LOGIN (Actualizado para detectar contraseña temporal) ---
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == request.Correo);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            {
                return Unauthorized(new { message = "Credenciales inválidas." });
            }

            // >>> NUEVA LÓGICA: Verificar si usa contraseña temporal <<<
            if (usuario.RequiereCambioPassword)
            {
                // 1. Verificar si ya expiró (24 horas)
                if (usuario.PasswordTemporalExpira.HasValue && usuario.PasswordTemporalExpira < DateTime.Now)
                {
                    return BadRequest(new { 
                        message = "Su contraseña temporal ha expirado. Por favor solicite una nueva.",
                        code = "TEMP_EXPIRED"
                    });
                }

                // 2. Si es válida, avisar al frontend que DEBE cambiarla
                // (No devolvemos el token todavía, o devolvemos un token limitado, 
                // pero para simplificar, devolvemos un código especial)
                return Ok(new { 
                    message = "Debe cambiar su contraseña temporal.",
                    code = "CHANGE_PASSWORD_REQUIRED",
                    correo = usuario.Correo 
                });
            }

            // Login normal
            var token = GenerateJwtToken(usuario);
            return Ok(new AuthResponseDto
            {
                Token = token,
                Nombre = usuario.Nombre,
                Rol = usuario.Rol
            });
        }

        // --- 3. RECUPERAR CONTRASEÑA (Genera temporal y envía correo) ---
        [HttpPost("recuperar-password")]
        public async Task<IActionResult> RecuperarPassword(SolicitarRecuperacionDto request)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == request.Correo);
            if (usuario == null) return BadRequest(new { message = "Correo no encontrado." });

            // 1. Generar contraseña temporal (8 caracteres aleatorios)
            string passwordTemporal = Path.GetRandomFileName().Replace(".", "").Substring(0, 8);
            
            // 2. Actualizar usuario en BD
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordTemporal);
            usuario.RequiereCambioPassword = true;
            usuario.PasswordTemporalExpira = DateTime.Now.AddHours(24); // Válida por 24h

            await _context.SaveChangesAsync();

            // 3. Enviar Correo
            string asunto = "Recuperación de Contraseña - MegaDelicias";
            string mensajeHtml = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
                    <h2 style='color: #1e40af;'>Recuperación de Acceso</h2>
                    <p>Hola <strong>{usuario.Nombre}</strong>,</p>
                    <p>Hemos recibido una solicitud para restablecer tu contraseña.</p>
                    <p>Tu contraseña temporal es:</p>
                    <div style='background: #f3f4f6; padding: 15px; border-radius: 8px; font-size: 20px; font-weight: bold; text-align: center; letter-spacing: 2px; border: 1px solid #ddd;'>
                        {passwordTemporal}
                    </div>
                    <p style='font-size: 12px; color: #666; margin-top: 20px;'>
                        * Esta contraseña es válida por 24 horas.<br>
                        * Al iniciar sesión, el sistema te pedirá crear una nueva contraseña segura.
                    </p>
                </div>
            ";

            try 
            {
                await _emailService.SendEmailAsync(usuario.Correo, asunto, mensajeHtml);
                return Ok(new { message = "Se ha enviado la contraseña temporal a tu correo." });
            }
            catch (Exception ex)
            {
                // En desarrollo, devolvemos el error para depurar. En producción, solo un log.
                return StatusCode(500, new { message = "Error enviando el correo.", error = ex.Message });
            }
        }

        // --- 4. CAMBIAR CONTRASEÑA (Finalizar recuperación) ---
        [HttpPost("cambiar-password")]
        public async Task<IActionResult> CambiarPassword(CambiarPasswordDto request)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == request.Correo);
            if (usuario == null) return BadRequest(new { message = "Usuario no encontrado." });

            // 1. Validar que la contraseña temporal sea correcta
            if (!BCrypt.Net.BCrypt.Verify(request.PasswordTemporal, usuario.PasswordHash))
            {
                return Unauthorized(new { message = "La contraseña temporal es incorrecta." });
            }

            // 2. Validar expiración (doble chequeo)
            if (usuario.RequiereCambioPassword && usuario.PasswordTemporalExpira < DateTime.Now)
            {
                return BadRequest(new { message = "La contraseña temporal ha expirado." });
            }

            // 3. Establecer la NUEVA contraseña definitiva
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NuevoPassword);
            usuario.RequiereCambioPassword = false; // Ya no es temporal
            usuario.PasswordTemporalExpira = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Contraseña actualizada exitosamente. Ya puedes iniciar sesión." });
        }

        // --- Helper para Token ---
        private string GenerateJwtToken(Usuario usuario)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key no configurada")));
            
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
                new Claim(JwtRegisteredClaimNames.Name, usuario.Nombre),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}