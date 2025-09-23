
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
        Task<Result<bool>> DeleteUserAsync(Guid id);
        Task<Result<LoginResponseDTO>> LoginUserAsync(LoginDTO user);
        Task<Result<bool>> LogoutUserAsync(string refresh_token);
        Task<Result<LoginResponseDTO>> RefreshLoginAsync(string old_refresh_token);
        Task<Result<UserDTO>> GetMe(Guid id);
    }
}
