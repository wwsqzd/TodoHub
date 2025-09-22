

using System.ComponentModel.DataAnnotations;

namespace TodoHub.Main.Core.DTOs.Request
{
    public class LoginDTO
    {
        [MaxLength(50)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
