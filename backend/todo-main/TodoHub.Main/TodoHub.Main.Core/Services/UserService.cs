using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Entities;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _config;
        private readonly IPasswordService _passwordService;
        private readonly IUserRepository _userRepository;
        private readonly AbstractValidator<RegisterDTO> _register_validator;
        private readonly AbstractValidator<LoginDTO> _login_validator;

        public UserService(IPasswordService passwordService, IUserRepository userRepository, AbstractValidator<RegisterDTO> register_validator, AbstractValidator<LoginDTO> login_validator, IConfiguration config)
        {

            _passwordService = passwordService;
            _userRepository = userRepository;
            _register_validator = register_validator;
            _login_validator = login_validator;
            _config = config;
        }

        // add user
        public async Task<Result<RegisterDTO>> AddUserAsync(RegisterDTO user)
        {
            ValidationResult resValidator = _register_validator.Validate(user);
            if (!resValidator.IsValid)
            {
                return Result<RegisterDTO>.Fail("Incorrect data entry");
            }
            user.Password = _passwordService.HashPassword(user.Password);
            if (await _userRepository.GetUserByEmailAsyncRepo(user.Email) != null)
            {
                return Result<RegisterDTO>.Fail("This user already exists");
            }
            await _userRepository.AddUserAsyncRepo(user);
            return Result<RegisterDTO>.Ok(user);
        }

        // login
        public async Task<Result<LoginResponseDTO>> LoginUserAsync(LoginDTO login_user)
        {
            ValidationResult resValidator = _login_validator.Validate(login_user);
            if (!resValidator.IsValid)
            {
                return Result<LoginResponseDTO>.Fail("Incorrect data entry");
            }

            // search user with email
            var user = await _userRepository.GetUserByEmailAsyncRepo(login_user.Email);

            // if the password is zero or not equal, we throw an error
            if (user == null)
                return Result<LoginResponseDTO>.Fail("Invalid email or password");
            bool isPasswordValid = _passwordService.VerifyPassword(user.Password, login_user.Password);

            if (!isPasswordValid)
                return Result<LoginResponseDTO>.Fail("Invalid password");

            // jwt claims
            var claims = new[]
            {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
            new Claim("UserId", user.Id.ToString())
            };
            // secret key for jwt
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // generate
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:Expire"]!)),
                signingCredentials: creds
            );

            var refreshtoken = await _passwordService.AddRefreshToken(user.Id);



            var response = new LoginResponseDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = token.ValidTo,
                RefreshToken = refreshtoken
            };

            return Result<LoginResponseDTO>.Ok(response);

        }

        // refresh token 
        public async Task<Result<LoginResponseDTO>> RefreshLoginAsync(string old_refresh_token)
        {
            
            var user = await _userRepository.GetUserByIdAsyncRepo(_passwordService.GetUserId(old_refresh_token));

            // jwt claims
            var claims = new[]
            {
            new Claim(ClaimTypes.Name, user!.Name),
            new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
            new Claim("UserId", user.Id.ToString())
            };
            // secret key for jwt
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // generate
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:Expire"]!)),
                signingCredentials: creds
            );

            var refreshtoken = await _passwordService.RefreshToken(old_refresh_token);

            var response = new LoginResponseDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = token.ValidTo,
                RefreshToken = refreshtoken
            };

            return Result<LoginResponseDTO>.Ok(response);
        }
        

        // get all users
        public async Task<List<UserDTO>> GetUsersAsync()
        {
            return await _userRepository.GetUsersAsyncRepo();
        }

        // seach with id
        public async Task<UserEntity?> GetUserByIdAsync(Guid id)
        {
            return await _userRepository.GetUserByIdAsyncRepo(id);
        }

        // search with email
        public async Task<UserEntity?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsyncRepo(email);
        }

        // delete user
        public async Task<Result<bool>> DeleteUserAsync(Guid id)
        {
            var user = await _userRepository.GetUserByIdAsyncRepo(id);
            if (user == null)
            {
                return Result<bool>.Fail("User does not exist");
            }
            await _userRepository.DeleteUserAsyncRepo(id);
            return Result<bool>.Ok(true);
        }

        // get profile
        public async Task<Result<UserDTO>> GetMe(Guid userId)
        {
            var user = await _userRepository.GetMeRepo(userId);
            if (user != null)
            {
                return Result<UserDTO>.Ok(user);
            }
            return Result<UserDTO>.Fail("User does not exist");
        }

        // logout
        public async Task<Result<bool>> LogoutUserAsync(string refresh_token)
        {
            try
            { 
                await _passwordService.RevokeRefreshToken(refresh_token);
                return Result<bool>.Ok(true);
            }
            catch (Exception ex) 
            {
                return Result<bool>.Fail($"{ex}");
            }
        }
    }
}
