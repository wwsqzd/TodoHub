

namespace TodoHub.Main.Core.DTOs.Request
{
    public class UpdateTodoDTO
    {
        public string? Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public bool? IsCompleted { get; set; }
    }
}
