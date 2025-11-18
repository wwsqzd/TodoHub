using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.DTOs.Response;

namespace TodoHub.Main.Core.Interfaces
{
    public interface IGoogleAuthService
    {
        string GetGoogleLoginUrl();
        Task<(string token, Result<LoginResponseDTO>)> HandleGoogleCallbackAsync(string code);
    }
}