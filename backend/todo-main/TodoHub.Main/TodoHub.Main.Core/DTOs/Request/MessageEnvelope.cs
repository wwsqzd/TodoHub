
namespace TodoHub.Main.Core.DTOs.Request
{
    public class MessageEnvelope
    {
        public string Command { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }
}
