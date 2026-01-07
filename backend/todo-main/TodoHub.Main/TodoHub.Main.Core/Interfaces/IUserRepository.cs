
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Entities;

namespace TodoHub.Main.Core.Interfaces
{
    public interface IUserRepository
    {
        Task AddUserAsyncRepo(RegisterDTO user, CancellationToken ct);
        Task<bool> DeleteUserAsyncRepo(Guid id, CancellationToken ct);
        Task<UserEntity?> GetUserByEmailAsyncRepo(string email, CancellationToken ct);
        Task<UserEntity?> GetUserByIdAsyncRepo(Guid id, CancellationToken ct);
        Task<List<UserDTO>> GetUsersAsyncRepo(CancellationToken ct);
        Task<UserDTO> GetMeRepo(Guid id, CancellationToken ct);
        Task<bool> IsUserAdminRepo(Guid id, CancellationToken ct);
        Task AddGoogleUserAsyncRepo(UserGoogleDTO user, CancellationToken ct);
        Task AddGitHubUserAsyncRepo(UserGitHubDTO user, CancellationToken ct);
        Task<bool> ChangeUserLanguageRepo(string language, Guid id, CancellationToken ct);

    }
}
