using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SharedModels.Clases;

namespace TritoteNic.Services
{
    public interface IJwtService
    {
        string GenerateToken(Usuario usuario, string secretKey, string issuer, string audience, int expirationMinutes);
    }

    public class JwtService : IJwtService
    {
        public string GenerateToken(Usuario usuario, string secretKey, string issuer, string audience, int expirationMinutes)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Email, usuario.EmailUsuario),
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Role, usuario.Rol?.NombreRol ?? "Usuario"),
                new Claim("IdRol", usuario.IdRol.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}


