
using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Entities;

namespace TodoHub.Main.Core.Interfaces
{
    public interface IUserService
    {
        Task<Result<List<UserDTO>>> GetUsersAsync(CancellationToken ct);
        Task<Result<UserEntity?>> GetUserByIdAsync(Guid id, CancellationToken ct);
        Task<Result<RegisterDTO>> AddUserAsync(RegisterDTO user, CancellationToken ct);
        Task<Result<Guid>> DeleteUserAsync(Guid id, CancellationToken ct);
        Task<(string token, Result<LoginResponseDTO>)> LoginUserAsync(LoginDTO user, CancellationToken ct);
        Task<Result<bool>> LogoutUserAsync(string refresh_token, CancellationToken ct);
        Task<(string token, Result<LoginResponseDTO>)> RefreshLoginAsync(string old_refresh_token, CancellationToken ct);
        Task<Result<UserDTO>> GetMe(Guid id, CancellationToken ct);
        Task<Result<bool>> IsUserAdmin(Guid id, CancellationToken ct);
        Task<Result<bool>> ChangeUserLanguage(ChangeLanguageDTO language_dto, Guid user_id, CancellationToken ct);
    }
}
