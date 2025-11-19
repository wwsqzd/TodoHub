// User Repository

using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
        public async Task AddUserAsyncRepo(RegisterDTO user)
        {
            var entity = _mapper.Map<UserEntity>(user);
            entity.AuthProvider = "Local";
            await _context.Users.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        // Add Google User
        public async Task AddGoogleUserAsyncRepo(UserGoogleDTO user)
        {
            var entity = _mapper.Map<UserEntity>(user);
            entity.AuthProvider = "Google";
            entity.GoogleId = user.GoogleId;

            await _context.Users.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        // Add GitHub User
        public async Task AddGitHubUserAsyncRepo(UserGitHubDTO user)
        {
            var entity = _mapper.Map<UserEntity>(user);
            entity.AuthProvider = "GitHub";
            entity.GitHubId = user.GitHubId;

            await _context.Users.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        // Get all users
        public async Task<List<UserDTO>> GetUsersAsyncRepo()
        {
            var users = await _context.Users.ToListAsync();
            var userDtos = _mapper.Map<List<UserDTO>>(users);
            return userDtos;
        }

        // Get user with id
        public async Task<UserEntity?> GetUserByIdAsyncRepo(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;
            return user;
        }

        // Get user with email
        public async Task<UserEntity?> GetUserByEmailAsyncRepo(string email)
        {
            var user =  await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;
            return user;
        }

        // delete user
        public async Task<bool> DeleteUserAsyncRepo(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // get profile
        public async Task<UserDTO> GetMeRepo(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            var userDto = _mapper.Map<UserDTO>(user);
            return userDto;
        }

        // is user admin?
        public async Task<bool> IsUserAdminRepo(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == id);
            if (user == null) return false;
            return user.IsAdmin;
        }
    }
}
