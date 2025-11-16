using System;

namespace SharedModels.Dto
{
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UsuarioDto Usuario { get; set; } = new UsuarioDto();
        public DateTime ExpiresAt { get; set; }
    }
}


