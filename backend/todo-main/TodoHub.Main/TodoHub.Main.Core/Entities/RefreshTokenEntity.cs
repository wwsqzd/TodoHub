

using System.ComponentModel.DataAnnotations;

namespace TodoHub.Main.Core.Entities
{
    public class RefreshTokenEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId {get;set;} 
        public string TokenHash { get; set; } = string.Empty;
        public DateTime Expires { get; set; } = DateTime.UtcNow.AddDays(7);
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime? Revoked { get; set; }
        public string? ReplacedByToken { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsActive => Revoked == null && !IsExpired;

    }
}
