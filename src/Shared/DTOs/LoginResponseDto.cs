using System;

namespace TimeStock.Shared.Dtos
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }

        public string Email { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }
}
