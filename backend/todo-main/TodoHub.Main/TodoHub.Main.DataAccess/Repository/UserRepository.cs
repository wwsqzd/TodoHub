// User Repository
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Entities;
using TodoHub.Main.Core.Interfaces;
using TodoHub.Main.DataAccess.Context;

namespace TodoHub.Main.DataAccess.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UserRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Add User 
        public async Task<bool> AddUserAsyncRepo(RegisterDTO user, CancellationToken ct)
        {
            Log.Information("AddUserAsyncRepo starting in UserRepository");
            var entity = _mapper.Map<UserEntity>(user);
            entity.AuthProvider = "Local";
            await _context.Users.AddAsync(entity,ct);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        // Add Google User
        public async Task AddGoogleUserAsyncRepo(UserGoogleDTO user, CancellationToken ct)
        {
            Log.Information("AddGoogleUserAsyncRepo starting in UserRepository");
            var entity = _mapper.Map<UserEntity>(user);
            entity.AuthProvider = "Google";
            entity.GoogleId = user.GoogleId;

            await _context.Users.AddAsync(entity,ct);
            await _context.SaveChangesAsync(ct);
        }

        // Add GitHub User
        public async Task AddGitHubUserAsyncRepo(UserGitHubDTO user, CancellationToken ct)
        {
            Log.Information("AddGitHubUserAsyncRepo starting in UserRepository");
            var entity = _mapper.Map<UserEntity>(user);
            entity.AuthProvider = "GitHub";
            entity.GitHubId = user.GitHubId;

            await _context.Users.AddAsync(entity,ct);
            await _context.SaveChangesAsync(ct);
        }

        // Get all users
        public async Task<List<UserDTO>> GetUsersAsyncRepo(CancellationToken ct)
        {
            Log.Information("GetUserAsyncRepo starting in UserRepository");
            var users = await _context.Users.ToListAsync(ct);
            var userDtos = _mapper.Map<List<UserDTO>>(users);
            return userDtos;
        }

        // Get user with id
        public async Task<UserEntity?> GetUserByIdAsyncRepo(Guid id, CancellationToken ct)
        {
            Log.Information("GetUserByIdAsyncRepo starting in UserRepository");
            var user = await _context.Users.FindAsync([id],ct);
            if (user == null) return null;
            return user;
        }

        // Get user with email
        public async Task<UserEntity?> GetUserByEmailAsyncRepo(string email, CancellationToken ct)
        {
            Log.Information("AddUserAsyncRepo starting in UserRepository");
            var user =  await _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
            if (user == null) return null;
            return user;
        }

        // delete user
        public async Task<bool> DeleteUserAsyncRepo(Guid id, CancellationToken ct)
        {
            Log.Information("DeleteUserAsyncRepo starting in UserRepository");
            var user = await _context.Users.FindAsync([id], ct);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        // get profile
        public async Task<UserDTO> GetMeRepo(Guid id, CancellationToken ct)
        {
            Log.Information("GetMeRepo starting in UserRepository");

            var user = await _context.Users.FindAsync([id],ct);
            var userDto = _mapper.Map<UserDTO>(user);
            userDto.Number_of_Todos = await _context.Todos.CountAsync(todo => todo.OwnerId == id, ct);
            return userDto;
        }

        // is user admin?
        public async Task<bool> IsUserAdminRepo(Guid id, CancellationToken ct)
        {
            Log.Information("IsUserAdminRepo starting in UserRepository");

            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == id, ct);
            if (user == null) return false;
            return user.IsAdmin;
        }

        // Change Language in User
        public async Task<bool> ChangeUserLanguageRepo(string language, Guid id, CancellationToken ct)
        {
            Log.Information("ChangeUserLanguageRepo starting in UserRepository");

            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == id, ct);
            if (user == null) return false;
            user.Interface_Language = language;
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}
