﻿

namespace TodoHub.Main.Core.DTOs.Response
{
    public class LoginResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        
    }
}
