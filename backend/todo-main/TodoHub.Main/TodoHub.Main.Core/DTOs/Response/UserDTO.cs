

namespace TodoHub.Main.Core.DTOs.Response
{
    public class UserDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public bool IsAdmin { get; set; }

        public DateTime CreatedAt { get; set; }
        public string AuthProvider { get; set; } = string.Empty;
        public string? PictureUrl { get; set; } = null;
        public string Interface_Language { get; set; } = string.Empty;

    }
}
