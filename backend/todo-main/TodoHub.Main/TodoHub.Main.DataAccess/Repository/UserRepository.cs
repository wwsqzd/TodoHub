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

        public async Task AddUserAsyncRepo(RegisterDTO user)
        {
            var entity = _mapper.Map<UserEntity>(user);
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
            return await _context.Users.FindAsync(id);
        }

        // Get user with email
        public async Task<UserEntity?> GetUserByEmailAsyncRepo(string email)
        {
            return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
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

        public async Task<bool> IsUserAdminRepo(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == id);
            if (user == null) return false;
            return user.IsAdmin;
        }
    }
}
