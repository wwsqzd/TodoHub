
namespace TodoHub.Main.Core.DTOs.Request
{
    public class RegisterDTO
    {
        // то что при регистрации ловим
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
