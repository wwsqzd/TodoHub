using System.ComponentModel.DataAnnotations;

namespace TodoHub.Main.Core.Entities
{
    public class TodoEntity
    {
        // What we store in the database
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; }
        public bool IsCompleted { get; set; } = false;
        public Guid OwnerId { get; set; }
    }
}
