using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.DTOs.Response;

namespace TodoHub.Main.Core.Interfaces
{
    public interface IGitHubAuthService
    {
        string GetGitHubLoginUrl();
        Task<(string token, Result<LoginResponseDTO>)> HandleGitHubCallbackAsync(string code, CancellationToken ct);
    }
}