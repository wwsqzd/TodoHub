

namespace TodoHub.Main.Core.DTOs.Response
{
    public class UserDTO
    {
        // дтошка. То, что отдаем пользователю 
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public bool IsAdmin { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
