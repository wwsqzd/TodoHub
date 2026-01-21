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
        private readonly IPasswordService password_serv;
        private readonly IJwtService jwt_serv;
        private readonly IUserRepository user_repo;
        private readonly AbstractValidator<RegisterDTO> _register_validator;
        private readonly AbstractValidator<LoginDTO> _login_validator;
        private readonly IRefreshTokenRepository tokens_repo;
        private readonly AbstractValidator<ChangeLanguageDTO> _language_validator;
        private readonly DbBulkhead _dbBulkhead;

        public UserService(
            IPasswordService passwordService,
            IJwtService jwtService,
            IUserRepository userRepository, 
            AbstractValidator<RegisterDTO> register_validator, 
            AbstractValidator<LoginDTO> login_validator, 
            IRefreshTokenRepository refreshTokenRepository,
            AbstractValidator<ChangeLanguageDTO> language_validator,
            DbBulkhead dbBulkhead
            )
        {
            password_serv = passwordService;
            jwt_serv = jwtService;
            user_repo = userRepository;
            _register_validator = register_validator;
            _login_validator = login_validator;
            tokens_repo = refreshTokenRepository;
            _language_validator = language_validator;
            _dbBulkhead = dbBulkhead;
        }

        // register user
        public async Task<Result<RegisterDTO>> AddUserAsync(RegisterDTO user, CancellationToken ct)
        {
            try
            {
                ValidationResult resValidator = _register_validator.Validate(user);
                if (!resValidator.IsValid)
                {
                    return Result<RegisterDTO>.Fail("Incorrect data entry");
                }
                if (await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => user_repo.GetUserByEmailAsyncRepo(user.Email, t), TimeSpan.FromSeconds(5), bct), ct) != null)
                {
                    return Result<RegisterDTO>.Fail("This user already exists");
                }
                var validatepass = Zxcvbn.Core.EvaluatePassword(user.Password);
                if (validatepass.Score < 3)
                {
                    return Result<RegisterDTO>.Fail("The password is too weak.");
                }

                user.Password = password_serv.HashPassword(user.Password);

                await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => user_repo.AddUserAsyncRepo(user, t), TimeSpan.FromSeconds(5), bct), ct);

                return Result<RegisterDTO>.Ok(user);
            }
            catch (BulkheadRejektedException ex)
            {
                return Result<RegisterDTO>.Fail($"Overloaded: {ex.Message}");
            }
            catch (Exception ex) {
                return Result<RegisterDTO>.Fail($"Exception: {ex.Message}");
            }
            
        }

        // login
        public async Task<(string token, Result<LoginResponseDTO>)> LoginUserAsync(LoginDTO login_user, CancellationToken ct)
        {
            try
            {
                // login validator
                ValidationResult resValidator = _login_validator.Validate(login_user);
                if (!resValidator.IsValid)
                {
                    return (string.Empty, Result<LoginResponseDTO>.Fail("Incorrect data entry"));
                }

                // search user with email
                var user = await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => user_repo.GetUserByEmailAsyncRepo(login_user.Email, t), TimeSpan.FromSeconds(5), bct), ct);

                // if the password is zero or not equal, we throw an error
                if (user == null)
                    return (string.Empty, Result<LoginResponseDTO>.Fail("Invalid email or password"));
                bool isPasswordValid = password_serv.VerifyPassword(user.Password, login_user.Password);

                if (!isPasswordValid)
                    return (string.Empty, Result<LoginResponseDTO>.Fail("Invalid password"));



                var token = jwt_serv.getJwtToken(user);
            

                string refreshtoken = await password_serv.AddRefreshToken(user.Id, ct);

                if (string.IsNullOrEmpty(refreshtoken))
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
            catch (BulkheadRejektedException ex)
            {
                return (string.Empty, Result<LoginResponseDTO>.Fail($"Overloaded: {ex.Message}"));
            }
            catch (Exception ex)
            {
                return (string.Empty, Result<LoginResponseDTO>.Fail($"Exception: {ex.Message}"));
            }
        }

        // refresh token 
        public async Task<(string token, Result<LoginResponseDTO>)> RefreshLoginAsync(string old_refresh_token, CancellationToken ct)
        {
            try
            {
                // validation
                var isValid = await password_serv.isRefreshTokenValid(old_refresh_token, ct);
                if (!isValid)
                {
                    return (string.Empty, Result<LoginResponseDTO>.Fail("Refresh token is Invalid"));
                }


                // user id in token
                var userIdInToken = await password_serv.GetUserId(old_refresh_token, ct);
                if (userIdInToken == null)
                {
                    return (string.Empty, Result<LoginResponseDTO>.Fail("UserID not found in token"));
                }
                // find user
                var user = await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => user_repo.GetUserByIdAsyncRepo(userIdInToken.Value, t), TimeSpan.FromSeconds(2), bct), ct);
                if (user == null)
                {
                    return (string.Empty, Result<LoginResponseDTO>.Fail("The user with this ID was not found."));
                }
                // JWT Token
                var jwt = jwt_serv.getJwtToken(user);
                var token = new JwtSecurityTokenHandler().WriteToken(jwt);

                // Refresh Token
                var newRefreshtoken = await password_serv.RefreshToken(old_refresh_token, userIdInToken.Value, ct);
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
            catch (BulkheadRejektedException ex)
            {
                return (string.Empty, Result<LoginResponseDTO>.Fail($"Overloaded: {ex.Message}"));
            }
            catch (Exception ex)
            {
                return (string.Empty, Result<LoginResponseDTO>.Fail($"Exception: {ex.Message}"));
            }
        }
        

        // get all users
        public async Task<Result<List<UserDTO>>> GetUsersAsync(CancellationToken ct)
        {
            try
            {
                var res = await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => user_repo.GetUsersAsyncRepo(t), TimeSpan.FromSeconds(5), bct), ct);
                return Result<List<UserDTO>>.Ok(res);
            }
            catch (BulkheadRejektedException ex)
            {
                return Result<List<UserDTO>>.Fail($"Overloaded: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Result<List<UserDTO>>.Fail($"Exception: {ex.Message}");
            }

        }

        // seach with id
        public async Task<Result<UserEntity?>> GetUserByIdAsync(Guid id, CancellationToken ct)
        {
            try
            {
                var res = await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => user_repo.GetUserByIdAsyncRepo(id, t), TimeSpan.FromSeconds(5), bct), ct);
                return Result<UserEntity?>.Ok(res);
            }
            catch (BulkheadRejektedException ex)
            {
                return Result<UserEntity?>.Fail($"Overloaded: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Result<UserEntity?>.Fail($"Exception: {ex.Message}");
            }
        }


        // delete user
        public async Task<Result<Guid>> DeleteUserAsync(Guid id, CancellationToken ct)
        {
            try
            {
                var user = await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => user_repo.GetUserByIdAsyncRepo(id, t), TimeSpan.FromSeconds(5), bct), ct);

                if (user == null)
                {
                    return Result<Guid>.Fail("User does not exist");
                }
                await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => user_repo.DeleteUserAsyncRepo(id, t), TimeSpan.FromSeconds(5), bct), ct);
                await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => tokens_repo.DeleteRefreshTokensByUserRepo(id, t), TimeSpan.FromSeconds(5), bct), ct);
                return Result<Guid>.Ok(id);
            }
            catch (BulkheadRejektedException ex)
            {
                return Result<Guid>.Fail($"Overloaded: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Result<Guid>.Fail($"Exception: {ex.Message}");
            }
        }

        // get profile
        public async Task<Result<UserDTO>> GetMe(Guid userId, CancellationToken ct)
        {
            try
            {
                var user = await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => user_repo.GetMeRepo(userId, t), TimeSpan.FromSeconds(5), bct), ct);
                if (user == null)
                {
                    return Result<UserDTO>.Fail("User does not exist");
                }
                return Result<UserDTO>.Ok(user);
            }
            catch (BulkheadRejektedException ex)
            {
                return Result<UserDTO>.Fail($"Overloaded: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Result<UserDTO>.Fail($"Exception: {ex.Message}");
            }
        }

        // logout
        public async Task<Result<bool>> LogoutUserAsync(string refresh_token, CancellationToken ct)
        {
            try
            {
                await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => password_serv.RevokeRefreshToken(refresh_token, t), TimeSpan.FromSeconds(3), bct), ct);
                return Result<bool>.Ok(true);
            }
            catch (BulkheadRejektedException ex)
            {
                return Result<bool>.Fail($"Overloaded: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail($"Exception: {ex.Message}");
            }
        }

        // validate user (admin or not)
        public async Task<Result<bool>> IsUserAdmin(Guid id, CancellationToken ct)
        {
            try
            {
                bool res = await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => user_repo.IsUserAdminRepo(id, t), TimeSpan.FromSeconds(5), bct), ct);
                return Result<bool>.Ok(res);
            }
            catch (BulkheadRejektedException ex)
            {
                return Result<bool>.Fail($"Overloaded: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail($"Exception: {ex.Message}");
            }
        }

        public async Task<Result<bool>> ChangeUserLanguage(ChangeLanguageDTO language_dto, Guid user_id, CancellationToken ct)
        {
            try
            {
                ValidationResult validationResult = _language_validator.Validate(language_dto);
                if (!validationResult.IsValid)
                {
                    return Result<bool>.Fail("Incorrect Data Entry");
                }
                var res = await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => user_repo.ChangeUserLanguageRepo(language_dto.Language, user_id, t), TimeSpan.FromSeconds(5), bct), ct);
                return Result<bool>.Ok(res);
            }
            catch (BulkheadRejektedException ex)
            {
                return Result<bool>.Fail($"Overloaded: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail($"Exception: {ex.Message}");
            }
        }
    }
}
