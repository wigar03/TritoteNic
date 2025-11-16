using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Clases;
using SharedModels.Dto;
using System.Security.Cryptography;
using System.Text;
using TritoteNic.Data;
using TritoteNic.Services;

namespace TritoteNic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TritoteContext.TritoteConext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthController(
            TritoteContext.TritoteConext context,
            ILogger<AuthController> logger,
            IJwtService jwtService,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
                {
                    return Unauthorized("Email y contraseña son requeridos.");
                }

                _logger.LogInformation($"Intento de login para email: {loginDto.Email}");

                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.EmailUsuario == loginDto.Email);

                if (usuario == null)
                {
                    _logger.LogWarning($"Usuario no encontrado: {loginDto.Email}");
                    return Unauthorized("Credenciales inválidas.");
                }

                if (usuario.EstadoUsuario != "Activo")
                {
                    _logger.LogWarning($"Intento de login con usuario inactivo: {loginDto.Email}");
                    return Unauthorized("Usuario inactivo.");
                }

                var hashedPassword = HashPassword(loginDto.Password);
                if (usuario.ContrasenaUsuario != hashedPassword && usuario.ContrasenaUsuario != loginDto.Password)
                {
                    _logger.LogWarning($"Contraseña incorrecta para: {loginDto.Email}");
                    return Unauthorized("Credenciales inválidas.");
                }

                usuario.UltimoAcceso = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var jwtKey = _configuration["Jwt:Key"] ?? "CHANGE_ME_IN_PRODUCTION";
                var jwtIssuer = _configuration["Jwt:Issuer"] ?? "TritoteNic";
                var jwtAudience = _configuration["Jwt:Audience"] ?? "TritoteNic_Users";
                var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

                var token = _jwtService.GenerateToken(usuario, jwtKey, jwtIssuer, jwtAudience, expirationMinutes);

                var response = new LoginResponseDto
                {
                    Token = token,
                    Usuario = new UsuarioDto
                    {
                        IdUsuario = usuario.IdUsuario,
                        NombreUsuario = usuario.NombreUsuario,
                        EmailUsuario = usuario.EmailUsuario,
                        EstadoUsuario = usuario.EstadoUsuario,
                        FechaCreacionUsuario = usuario.FechaCreacionUsuario,
                        UltimoAcceso = usuario.UltimoAcceso,
                        IdRol = usuario.IdRol,
                        NombreRol = usuario.Rol?.NombreRol
                    },
                    ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
                };

                _logger.LogInformation($"Login exitoso para: {loginDto.Email}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al procesar login: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno del servidor.");
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}


