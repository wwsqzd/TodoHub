namespace TodoHub.Main.Core.Entities
{
    public class UserEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public bool IsAdmin { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string AuthProvider { get; set; } = string.Empty;
        public string GoogleId { get; set; } = string.Empty;
        public string? PictureUrl { get; set; } = null;
        public string GitHubId { get; set; } = string.Empty;
        public string Interface_Language { get; set; } = "en";
        public double Average_completion_time { get; set; } = 0;
        public int Complated_Todo { get; set; } = 0;
    }
}
