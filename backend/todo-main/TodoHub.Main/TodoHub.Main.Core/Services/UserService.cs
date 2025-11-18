using FluentValidation;
using FluentValidation.Results;
using System.IdentityModel.Tokens.Jwt;
using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Entities;
using TodoHub.Main.Core.Interfaces;
using TodoHub.Main.DataAccess.Interfaces;

namespace TodoHub.Main.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IPasswordService _passwordService;
        private readonly IJwtService _jwtService;
        private readonly IUserRepository _userRepository;
        private readonly AbstractValidator<RegisterDTO> _register_validator;
        private readonly AbstractValidator<LoginDTO> _login_validator;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public UserService(
            IPasswordService passwordService,
            IJwtService jwtService,
            IUserRepository userRepository, 
            AbstractValidator<RegisterDTO> register_validator, 
            AbstractValidator<LoginDTO> login_validator, 
            IRefreshTokenRepository refreshTokenRepository
            )
        {
            _passwordService = passwordService;
            _jwtService = jwtService;
            _userRepository = userRepository;
            _register_validator = register_validator;
            _login_validator = login_validator;
            _refreshTokenRepository = refreshTokenRepository;
        }

        // register user
        public async Task<Result<RegisterDTO>> AddUserAsync(RegisterDTO user)
        {
            ValidationResult resValidator = _register_validator.Validate(user);
            if (!resValidator.IsValid)
            {
                return Result<RegisterDTO>.Fail("Incorrect data entry");
            }
            if (await _userRepository.GetUserByEmailAsyncRepo(user.Email) != null)
            {
                return Result<RegisterDTO>.Fail("This user already exists");
            }
            var validatepass = Zxcvbn.Core.EvaluatePassword(user.Password);
            if (validatepass.Score < 3)
            {
                return Result<RegisterDTO>.Fail("The password is too weak.");
            }

            user.Password = _passwordService.HashPassword(user.Password);
            
            await _userRepository.AddUserAsyncRepo(user);
            return Result<RegisterDTO>.Ok(user);
        }

        // login
        public async Task<(string token, Result<LoginResponseDTO>)> LoginUserAsync(LoginDTO login_user)
        {
            // login validator
            ValidationResult resValidator = _login_validator.Validate(login_user);
            if (!resValidator.IsValid)
            {
                return ("", Result<LoginResponseDTO>.Fail("Incorrect data entry"));
            }

            // search user with email
            var user = await _userRepository.GetUserByEmailAsyncRepo(login_user.Email);

            // if the password is zero or not equal, we throw an error
            if (user == null)
                return ("", Result<LoginResponseDTO>.Fail("Invalid email or password"));
            bool isPasswordValid = _passwordService.VerifyPassword(user.Password, login_user.Password);

            if (!isPasswordValid)
                return ("", Result<LoginResponseDTO>.Fail("Invalid password"));



            var token = _jwtService.getJwtToken(user);
            

            var refreshtoken = await _passwordService.AddRefreshToken(user.Id);

            if (refreshtoken == null)
            {
                return (string.Empty, Result<LoginResponseDTO>.Fail("Refresh Token is invalid"));
            }

            var response = new LoginResponseDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = token.ValidTo,
            };

            return (refreshtoken, Result<LoginResponseDTO>.Ok(response));


        }

        // refresh token 
        public async Task<(string token, Result<LoginResponseDTO>)> RefreshLoginAsync(string old_refresh_token)
        {
            
            // validation
            var isValid = await _passwordService.isRefreshTokenValid(old_refresh_token);
            if (!isValid)
            {
                return (string.Empty, Result<LoginResponseDTO>.Fail("Refresh token is Invalid"));
            }


            // user id in token
            var userIdInToken = await _passwordService.GetUserId(old_refresh_token);
            if (userIdInToken == null)
            {
                return (string.Empty, Result<LoginResponseDTO>.Fail("UserID not found in token"));
            }
            // find user
            var user = await _userRepository.GetUserByIdAsyncRepo(userIdInToken.Value);
            if (user == null)
            {
                return (string.Empty, Result<LoginResponseDTO>.Fail("The user with this ID was not found."));
            }
            // JWT Token
            var jwt = _jwtService.getJwtToken(user);
            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            // Refresh Token
            var newRefreshtoken = await _passwordService.RefreshToken(old_refresh_token, userIdInToken.Value);
            if (newRefreshtoken == null)
            {
                return (string.Empty, Result<LoginResponseDTO>.Fail("Refresh Token is Invalid"));
            }

            //response
            var responseNew = new LoginResponseDTO
            {
                Token = token,
                ExpiresAt = jwt.ValidTo,
            };

            return (newRefreshtoken, Result<LoginResponseDTO>.Ok(responseNew));
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
        public async Task<Result<Guid>> DeleteUserAsync(Guid id)
        {
            var user = await _userRepository.GetUserByIdAsyncRepo(id);
            if (user == null)
            {
                return Result<Guid>.Fail("User does not exist");
            }
            await _userRepository.DeleteUserAsyncRepo(id);
            await _refreshTokenRepository.DeleteRefreshTokensByUserRepo(id);
            return Result<Guid>.Ok(id);
        }

        // get profile
        public async Task<Result<UserDTO>> GetMe(Guid userId)
        {
            var user = await _userRepository.GetMeRepo(userId);
            if (user == null)
            {
                return Result<UserDTO>.Fail("User does not exist");    
            }
            return Result<UserDTO>.Ok(user);
            
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

        // validate user (admin or not)
        public async Task<Result<bool>> IsUserAdmin(Guid id)
        {
            try
            {
                bool res = await _userRepository.IsUserAdminRepo(id);
                return Result<bool>.Ok(res);
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail($"{ex}");
            }
        }
    }
}
