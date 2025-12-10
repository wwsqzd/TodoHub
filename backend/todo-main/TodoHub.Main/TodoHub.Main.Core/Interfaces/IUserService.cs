
using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Entities;

namespace TodoHub.Main.Core.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDTO>> GetUsersAsync();
        Task<UserEntity?> GetUserByIdAsync(Guid id);
        Task<UserEntity?> GetUserByEmailAsync(string email);
        Task<Result<RegisterDTO>> AddUserAsync(RegisterDTO user);
        Task<Result<Guid>> DeleteUserAsync(Guid id);
        Task<(string token, Result<LoginResponseDTO>)> LoginUserAsync(LoginDTO user);
        Task<Result<bool>> LogoutUserAsync(string refresh_token);
        Task<(string token, Result<LoginResponseDTO>)> RefreshLoginAsync(string old_refresh_token);
        Task<Result<UserDTO>> GetMe(Guid id);
        Task<Result<bool>> IsUserAdmin(Guid id);
        Task<Result<bool>> ChangeUserLanguage(ChangeLanguageDTO language_dto, Guid user_id);
    }
}
