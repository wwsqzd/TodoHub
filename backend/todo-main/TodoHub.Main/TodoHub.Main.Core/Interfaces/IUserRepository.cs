
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Entities;

namespace TodoHub.Main.Core.Interfaces
{
    public interface IUserRepository
    {
        Task AddUserAsyncRepo(RegisterDTO user);
        Task<bool> DeleteUserAsyncRepo(Guid id);
        Task<UserEntity?> GetUserByEmailAsyncRepo(string email);
        Task<UserEntity?> GetUserByIdAsyncRepo(Guid id);
        Task<List<UserDTO>> GetUsersAsyncRepo();
        Task<UserDTO> GetMeRepo(Guid id);
        Task<bool> IsUserAdminRepo(Guid id);
    }
}
