using FluentValidation;
using FluentValidation.Results;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Entities;
using TodoHub.Main.Core.Interfaces;
using TodoHub.Main.Core.Services;
using TodoHub.Main.DataAccess.Interfaces;

namespace TodoHub.Tests.Services
{
    public class UserServiceTests
    {
        private static ValidationResult Valid() => new();

        private static ValidationResult Invalid(params ValidationFailure[] failures)
            => new ValidationResult(failures);

        public sealed class FakeValidator<T> : AbstractValidator<T>
        {
            private readonly ValidationResult _result;

            public FakeValidator(ValidationResult result) => _result = result;

            public override ValidationResult Validate(ValidationContext<T> context) => _result;

            public override Task<ValidationResult> ValidateAsync(
                ValidationContext<T> context,
                CancellationToken cancellation = default)
                => Task.FromResult(_result);
        }

        private (
            UserService sut,
            Mock<IPasswordService> password_serv,
            Mock<IJwtService> jwt_serv,
            Mock<IUserRepository> user_repo,
            Mock<IRefreshTokenRepository> tokens_repo

            ) CreateSut(
                ValidationResult? register_validation = null, ValidationResult? login_validation = null, ValidationResult? language_validation = null
            )
        {
            var user_repo = new Mock<IUserRepository>(MockBehavior.Strict);
            var tokens_repo = new Mock<IRefreshTokenRepository>(MockBehavior.Strict);
            var jwt_serv = new Mock<IJwtService>(MockBehavior.Strict);
            var password_serv = new Mock<IPasswordService>(MockBehavior.Strict);

            var registerValidator = new FakeValidator<RegisterDTO>(register_validation ?? Valid());
            var loginValidator = new FakeValidator<LoginDTO>(login_validation ?? Valid());
            var languageValidator = new FakeValidator<ChangeLanguageDTO>(language_validation ?? Valid());

            var dbBulkhead = new DbBulkhead();

            var sut = new UserService(
                password_serv.Object,
                jwt_serv.Object,
                user_repo.Object,
                registerValidator,
                loginValidator,
                tokens_repo.Object,
                languageValidator, dbBulkhead
            );

            return (sut, password_serv, jwt_serv, user_repo, tokens_repo);

        }


        [Fact]
        public async Task AddUserAsync_WhenValidationFails_ReturnsFail_AndCallAlJustOnce()
        {
            // Arrange
            var dto = new RegisterDTO { Email = "a@b.com", Password = "123" };

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut(register_validation: Invalid(new ValidationFailure("Email", "bad")));

            // Act

            var res = await sut.AddUserAsync(dto, CancellationToken.None);

            // Assert
            Assert.False(res.Success);
            Assert.Equal("Incorrect data entry", res.Error);

            user_repo.VerifyNoOtherCalls();
            tokens_repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task AddUserAsync_WhenUserAlreadyExists_ReturnsFail()
        {
            // Arrange
            var dto = new RegisterDTO { Email = "exists@b.com", Password = "StrongPass123!" };

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            user_repo.Setup(r => r.GetUserByEmailAsyncRepo(dto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserEntity());

            // Act
            var res = await sut.AddUserAsync(dto, CancellationToken.None);

            // Assert
            Assert.False(res.Success);
            Assert.Equal("This user already exists", res.Error);

            user_repo.Verify(r => r.GetUserByEmailAsyncRepo(dto.Email, It.IsAny<CancellationToken>()), Times.Once);

            user_repo.VerifyNoOtherCalls();
        }
        

        [Fact]
        public async Task AddUserAsync_WhenPasswordIsWeak_ReturnsFail()
        {
            // Arrange
            var dto = new RegisterDTO { Email = "new@b.com", Password = "12345" };

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            user_repo.Setup(r => r.GetUserByEmailAsyncRepo(dto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserEntity?)null);

            // Act
            var res = await sut.AddUserAsync(dto, CancellationToken.None);

            // Assert
            Assert.False(res.Success);
            Assert.Equal("The password is too weak.", res.Error);

            user_repo.Verify(r => r.GetUserByEmailAsyncRepo(dto.Email, It.IsAny<CancellationToken>()), Times.Once);
            user_repo.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Happy path AddUserAsync 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddUserAsync_WhenOk_HashesPassword_AndAddsUser_ReturnsOk()
        {
            var dto = new RegisterDTO { Email = "new@b.com", Password = "CorrectHorseBatteryStaple!!123" };

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            user_repo.Setup(r => r.GetUserByEmailAsyncRepo(dto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserEntity?)null);

            password_serv.Setup(p => p.HashPassword(It.IsAny<string>()))
                .Returns("HASHED");

            user_repo.Setup(r => r.AddUserAsyncRepo(dto, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var res = await sut.AddUserAsync(dto, CancellationToken.None);

            Assert.True(res.Success);
            Assert.Equal("HASHED", res.Value.Password);

            password_serv.Verify(p => p.HashPassword(It.Is<string>(s => s != "HASHED")), Times.Once);

            user_repo.Verify(r => r.AddUserAsyncRepo(dto, It.IsAny<CancellationToken>()), Times.Once);
            user_repo.Verify(r => r.GetUserByEmailAsyncRepo(dto.Email, It.IsAny<CancellationToken>()), Times.Once);

            user_repo.VerifyNoOtherCalls();
            password_serv.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task AddUserAsync_RepoThrows_ReturnsFail()
        {
            var dto = new RegisterDTO { Email = "new@b.com", Password = "CorrectHorseBatteryStaple!!123" };

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            user_repo.Setup(r => r.GetUserByEmailAsyncRepo(dto.Email, It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("DB is down"));

            var res = await sut.AddUserAsync(dto, CancellationToken.None);

            Assert.False(res.Success);
            Assert.Contains("Exception:", res.Error);
            Assert.Contains("DB is down", res.Error);

            user_repo.Verify(r => r.GetUserByEmailAsyncRepo(dto.Email, It.IsAny<CancellationToken>()), Times.Once);

            user_repo.VerifyNoOtherCalls();
        }



        [Fact]
        public async Task LoginUserAsync_WhenValidationFails_ReturnsFail()
        {
            var dto = new LoginDTO { Email = "a@b.com", Password = "x" };

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut(login_validation: Invalid(new ValidationFailure("Email", "bad")));


            var (refresh, res) = await sut.LoginUserAsync(dto, CancellationToken.None);

            Assert.Equal(string.Empty, refresh);
            Assert.False(res.Success);
            Assert.Equal("Incorrect data entry", res.Error);

            user_repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task LoginUserAsync_WhenUserNotFound_ReturnsFailInvalidEmailOrPassword()
        {
            var dto = new LoginDTO { Email = "no@b.com", Password = "pass" };


            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            user_repo.Setup(r => r.GetUserByEmailAsyncRepo(dto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserEntity?)null);

            var (refresh, res) = await sut.LoginUserAsync(dto, CancellationToken.None);

            Assert.Equal(string.Empty, refresh);
            Assert.False(res.Success);
            Assert.Equal("Invalid email or password", res.Error);

            user_repo.Verify(r => r.GetUserByEmailAsyncRepo(dto.Email, It.IsAny<CancellationToken>()), Times.Once);
            user_repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task LoginUserAsync_WhenPasswordInvalid_ReturnsFailInvalidPassword()
        {
            var dto = new LoginDTO { Email = "u@b.com", Password = "wrong" };

            var user = new UserEntity { Id = Guid.NewGuid(), Password = "HASH" };

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            user_repo.Setup(r => r.GetUserByEmailAsyncRepo(dto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            password_serv.Setup(p => p.VerifyPassword(user.Password, dto.Password))
                .Returns(false);

            var (refresh, res) = await sut.LoginUserAsync(dto, CancellationToken.None);

            Assert.Equal(string.Empty, refresh);
            Assert.False(res.Success);
            Assert.Equal("Invalid password", res.Error);

            user_repo.Verify(r => r.GetUserByEmailAsyncRepo(dto.Email, It.IsAny<CancellationToken>()), Times.Once);
            password_serv.Verify(r => r.VerifyPassword(user.Password, dto.Password), Times.Once);

            user_repo.VerifyNoOtherCalls();
            password_serv.VerifyNoOtherCalls();

        }

        [Fact]
        public async Task LoginUserAsync_WhenRefreshTokenNull_ReturnsFail()
        {
            var dto = new LoginDTO { Email = "u@b.com", Password = "ok" };

            var user = new UserEntity { Id = Guid.NewGuid(), Password = "HASH" };

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            user_repo.Setup(r => r.GetUserByEmailAsyncRepo(dto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            password_serv.Setup(p => p.VerifyPassword(user.Password, dto.Password))
                .Returns(true);

            jwt_serv.Setup(j => j.getJwtToken(user))
                .Returns(new JwtSecurityToken(expires: DateTime.UtcNow.AddMinutes(60)));

            password_serv.Setup(p => p.AddRefreshToken(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(string.Empty);

            var (refresh, res) = await sut.LoginUserAsync(dto, CancellationToken.None);

            Assert.Equal(string.Empty, refresh);
            Assert.False(res.Success);
            Assert.Equal("Refresh Token is invalid", res.Error);

            user_repo.Verify(r => r.GetUserByEmailAsyncRepo(dto.Email, It.IsAny<CancellationToken>()), Times.Once);
            password_serv.Verify(p => p.VerifyPassword(user.Password, dto.Password), Times.Once);
            jwt_serv.Verify(j => j.getJwtToken(user), Times.Once);
            password_serv.Verify(p => p.AddRefreshToken(user.Id, It.IsAny<CancellationToken>()), Times.Once);

            user_repo.VerifyNoOtherCalls();
            password_serv.VerifyNoOtherCalls();
            jwt_serv.VerifyNoOtherCalls();

        }

        [Fact]
        public async Task LoginUserAsync_WhenOk_ReturnsRefreshToken_AndOkResponse()
        {
            var dto = new LoginDTO { Email = "u@b.com", Password = "ok" };

            var user = new UserEntity { Id = Guid.NewGuid(), Password = "HASH" };


            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            user_repo.Setup(r => r.GetUserByEmailAsyncRepo(dto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            password_serv.Setup(p => p.VerifyPassword(user.Password, dto.Password))
                .Returns(true);

            var jwt = new JwtSecurityToken(expires: DateTime.UtcNow.AddMinutes(60));
            jwt_serv.Setup(j => j.getJwtToken(user)).Returns(jwt);

            password_serv.Setup(p => p.AddRefreshToken(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync("REFRESH");

            var (refresh, res) = await sut.LoginUserAsync(dto, CancellationToken.None);

            Assert.Equal("REFRESH", refresh);
            Assert.True(res.Success);
            Assert.False(string.IsNullOrWhiteSpace(res.Value.Token));
            Assert.Equal(jwt.ValidTo, res.Value.ExpiresAt);


            user_repo.Verify(r => r.GetUserByEmailAsyncRepo(dto.Email, It.IsAny<CancellationToken>()), Times.Once);
            password_serv.Verify(p => p.VerifyPassword(user.Password, dto.Password), Times.Once);
            jwt_serv.Verify(j => j.getJwtToken(user), Times.Once);
            password_serv.Verify(p => p.AddRefreshToken(user.Id, It.IsAny<CancellationToken>()), Times.Once);

            user_repo.VerifyNoOtherCalls();
            password_serv.VerifyNoOtherCalls();
            jwt_serv.VerifyNoOtherCalls();

        }

        [Fact]
        public async Task LoginUserAsync_RepoThrows_ReturnsFailOverloaded()
        {
            var dto = new LoginDTO { Email = "u@b.com", Password = "ok" };

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            user_repo.Setup(r => r.GetUserByEmailAsyncRepo(dto.Email, It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("DB is down"));

            var (refresh, res) = await sut.LoginUserAsync(dto, CancellationToken.None);

            Assert.Equal(string.Empty, refresh);
            Assert.False(res.Success);
            Assert.Contains("Exception:", res.Error);
            Assert.Contains("DB is down", res.Error);

            user_repo.Verify(r => r.GetUserByEmailAsyncRepo(dto.Email, It.IsAny<CancellationToken>()), Times.Once);
            user_repo.VerifyNoOtherCalls();
        }



        [Fact]
        public async Task RefreshLoginAsync_WhenTokenInvalid_ReturnsFail()
        {
            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            password_serv.Setup(p => p.isRefreshTokenValid("OLD", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var (newRefresh, res) = await sut.RefreshLoginAsync("OLD", CancellationToken.None);

            Assert.Equal(string.Empty, newRefresh);
            Assert.False(res.Success);
            Assert.Equal("Refresh token is Invalid", res.Error);

            password_serv.Verify(p => p.isRefreshTokenValid("OLD", It.IsAny<CancellationToken>()), Times.Once);
            password_serv.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task RefreshLoginAsync_WhenUserIdNull_ReturnsFail()
        {
            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            password_serv.Setup(p => p.isRefreshTokenValid("OLD", It.IsAny<CancellationToken>())).ReturnsAsync(true);
            password_serv.Setup(p => p.GetUserId("OLD", It.IsAny<CancellationToken>())).ReturnsAsync((Guid?)null);

            var (newRefresh, res) = await sut.RefreshLoginAsync("OLD", CancellationToken.None);

            Assert.Equal(string.Empty, newRefresh);
            Assert.False(res.Success);
            Assert.Equal("UserID not found in token", res.Error);

            password_serv.Verify(p => p.isRefreshTokenValid("OLD", It.IsAny<CancellationToken>()), Times.Once);
            password_serv.Verify(p => p.GetUserId("OLD", It.IsAny<CancellationToken>()), Times.Once);
            password_serv.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task RefreshLoginAsync_WhenUserNotFound_ReturnsFail()
        {
            var userId = Guid.NewGuid();

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            password_serv.Setup(p => p.isRefreshTokenValid("OLD", It.IsAny<CancellationToken>())).ReturnsAsync(true);
            password_serv.Setup(p => p.GetUserId("OLD", It.IsAny<CancellationToken>())).ReturnsAsync(userId);

            user_repo.Setup(r => r.GetUserByIdAsyncRepo(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserEntity?)null);

            var (newRefresh, res) = await sut.RefreshLoginAsync("OLD", CancellationToken.None);

            Assert.Equal(string.Empty, newRefresh);
            Assert.False(res.Success);
            Assert.Equal("The user with this ID was not found.", res.Error);
        }

        [Fact]
        public async Task RefreshLoginAsync_WhenNewRefreshNull_ReturnsFail()
        {
            var userId = Guid.NewGuid();
            var user = new UserEntity { Id = userId, Password = "HASH" };

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            password_serv.Setup(p => p.isRefreshTokenValid("OLD", It.IsAny<CancellationToken>())).ReturnsAsync(true);
            password_serv.Setup(p => p.GetUserId("OLD", It.IsAny<CancellationToken>())).ReturnsAsync(userId);

            user_repo.Setup(r => r.GetUserByIdAsyncRepo(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var jwt = new JwtSecurityToken(expires: DateTime.UtcNow.AddMinutes(60));
            jwt_serv.Setup(j => j.getJwtToken(user)).Returns(jwt);

            password_serv.Setup(p => p.RefreshToken("OLD", userId, It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);

            var (newRefresh, res) = await sut.RefreshLoginAsync("OLD", CancellationToken.None);

            Assert.Equal(string.Empty, newRefresh);
            Assert.False(res.Success);
            Assert.Equal("Refresh Token is Invalid", res.Error);

            password_serv.Verify(p => p.isRefreshTokenValid("OLD", It.IsAny<CancellationToken>()), Times.Once);
            password_serv.Verify(p => p.GetUserId("OLD", It.IsAny<CancellationToken>()), Times.Once);
            user_repo.Verify(r => r.GetUserByIdAsyncRepo(userId, It.IsAny<CancellationToken>()), Times.Once);
            jwt_serv.Verify(j => j.getJwtToken(user), Times.Once);
            password_serv.Verify(p => p.RefreshToken("OLD", userId, It.IsAny<CancellationToken>()), Times.Once);

            password_serv.VerifyNoOtherCalls();
            user_repo.VerifyNoOtherCalls();
            jwt_serv.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task RefreshLoginAsync_WhenOk_ReturnsNewRefreshAndOkResponse()
        {
            var userId = Guid.NewGuid();
            var user = new UserEntity { Id = userId, Password = "HASH" };

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            password_serv.Setup(p => p.isRefreshTokenValid("OLD", It.IsAny<CancellationToken>())).ReturnsAsync(true);
            password_serv.Setup(p => p.GetUserId("OLD", It.IsAny<CancellationToken>())).ReturnsAsync(userId);

            user_repo.Setup(r => r.GetUserByIdAsyncRepo(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var jwt = new JwtSecurityToken(expires: DateTime.UtcNow.AddMinutes(10));
            jwt_serv.Setup(j => j.getJwtToken(user)).Returns(jwt);

            password_serv.Setup(p => p.RefreshToken("OLD", userId, It.IsAny<CancellationToken>())).ReturnsAsync("NEW_REFRESH");

            var (newRefresh, res) = await sut.RefreshLoginAsync("OLD", CancellationToken.None);

            Assert.Equal("NEW_REFRESH", newRefresh);
            Assert.True(res.Success);
            Assert.False(string.IsNullOrWhiteSpace(res.Value.Token));
            Assert.Equal(jwt.ValidTo, res.Value.ExpiresAt);


            password_serv.Verify(p => p.isRefreshTokenValid("OLD", It.IsAny<CancellationToken>()), Times.Once);
            password_serv.Verify(p => p.GetUserId("OLD", It.IsAny<CancellationToken>()), Times.Once);
            user_repo.Verify(r => r.GetUserByIdAsyncRepo(userId, It.IsAny<CancellationToken>()), Times.Once);
            jwt_serv.Verify(j => j.getJwtToken(user), Times.Once);
            password_serv.Verify(p => p.RefreshToken("OLD", userId, It.IsAny<CancellationToken>()), Times.Once);

            password_serv.VerifyNoOtherCalls();
            user_repo.VerifyNoOtherCalls();
            jwt_serv.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task RefreshLoginAsync_Repothrows_ReturnsFail()
        {

            var userId = Guid.NewGuid();

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            password_serv.Setup(p => p.isRefreshTokenValid("OLD", It.IsAny<CancellationToken>())).ReturnsAsync(true);
            password_serv.Setup(p => p.GetUserId("OLD", It.IsAny<CancellationToken>())).ReturnsAsync(userId);

            user_repo.Setup(r => r.GetUserByIdAsyncRepo(userId, It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("DB is down"));

            var (newRefresh, res) = await sut.RefreshLoginAsync("OLD", CancellationToken.None);

            Assert.Equal(string.Empty, newRefresh);
            Assert.False(res.Success);
            Assert.Contains("Exception:", res.Error);
            Assert.Contains("DB is down", res.Error);

            password_serv.Verify(p => p.isRefreshTokenValid("OLD", It.IsAny<CancellationToken>()), Times.Once);
            password_serv.Verify(p => p.GetUserId("OLD", It.IsAny<CancellationToken>()), Times.Once);

            password_serv.VerifyNoOtherCalls();

        }



        [Fact]
        public async Task GetUsersAsync_WhenOk_ReturnsOk()
        {
            var list = new List<UserDTO> { new UserDTO() };

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            user_repo.Setup(r => r.GetUsersAsyncRepo(It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var res = await sut.GetUsersAsync(CancellationToken.None);

            Assert.True(res.Success);
            Assert.Same(list, res.Value);

            user_repo.Verify(r => r.GetUsersAsyncRepo(It.IsAny<CancellationToken>()), Times.Once);
            user_repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetUsersAsync_RepoThrows_ReturnsFailOverloaded()
        {

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            user_repo.Setup(r => r.GetUsersAsyncRepo(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("DB is down"));

            var res = await sut.GetUsersAsync(CancellationToken.None);

            Assert.False(res.Success);
            Assert.Contains("Exception:", res.Error);
            Assert.Contains("DB is down", res.Error);

            user_repo.Verify(r => r.GetUsersAsyncRepo(It.IsAny<CancellationToken>()), Times.Once);
            user_repo.VerifyNoOtherCalls();
        }



        [Fact]
        public async Task GetUserByIdAsync_WhenOk_ReturnsOk()
        {
            var id = Guid.NewGuid();
            var user = new UserEntity { Id = id };

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            user_repo.Setup(r => r.GetUserByIdAsyncRepo(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var res = await sut.GetUserByIdAsync(id, CancellationToken.None);

            Assert.True(res.Success);
            Assert.Same(user, res.Value);

            user_repo.Verify(r => r.GetUserByIdAsyncRepo(id, It.IsAny<CancellationToken>()), Times.Once);
            user_repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetUserByIdAsync_Repothrows_ReturnsFail()
        {
            var userId = Guid.NewGuid();

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            user_repo.Setup(r => r.GetUserByIdAsyncRepo(userId, It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("DB is down"));

            var res = await sut.GetUserByIdAsync(userId, CancellationToken.None);
            Console.WriteLine(res.Error);
            Assert.False(res.Success);
            Assert.Contains("Exception:", res.Error);
            Assert.Contains("DB is down", res.Error);


            user_repo.Verify(r => r.GetUserByIdAsyncRepo(userId, It.IsAny<CancellationToken>()), Times.Once);
            user_repo.VerifyNoOtherCalls();

        }


        [Fact]
        public async Task DeleteUserAsync_WhenUserNotFound_ReturnsFail()
        {
            var id = Guid.NewGuid();

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            user_repo.Setup(r => r.GetUserByIdAsyncRepo(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserEntity?)null);

            var res = await sut.DeleteUserAsync(id, CancellationToken.None);

            Assert.False(res.Success);
            Assert.Equal("User does not exist", res.Error);

            user_repo.Verify(r => r.GetUserByIdAsyncRepo(id, It.IsAny<CancellationToken>()), Times.Once);
            user_repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task DeleteUserAsync_WhenOk_DeletesUserAndRefreshTokens_ReturnsOk()
        {
            var id = Guid.NewGuid();

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            user_repo.Setup(r => r.GetUserByIdAsyncRepo(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserEntity { Id = id });

            user_repo.Setup(r => r.DeleteUserAsyncRepo(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            tokens_repo.Setup(r => r.DeleteRefreshTokensByUserRepo(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var res = await sut.DeleteUserAsync(id, CancellationToken.None);

            Assert.True(res.Success);
            Assert.Equal(id, res.Value);

            user_repo.Verify(r => r.DeleteUserAsyncRepo(id, It.IsAny<CancellationToken>()), Times.Once);
            tokens_repo.Verify(r => r.DeleteRefreshTokensByUserRepo(id, It.IsAny<CancellationToken>()), Times.Once);
            user_repo.Verify(r => r.GetUserByIdAsyncRepo(id, It.IsAny<CancellationToken>()), Times.Once);
            user_repo.VerifyNoOtherCalls();
            tokens_repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task DeleteUserAsync_RepoThrows_ReturnsFailOverloaded()
        {
            var userId = Guid.NewGuid();
            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            user_repo.Setup(r => r.GetUserByIdAsyncRepo(userId, It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("DB is down"));

            var res = await sut.DeleteUserAsync(userId, CancellationToken.None);

            Assert.False(res.Success);
            Assert.Contains("Exception:", res.Error);
            Assert.Contains("DB is down", res.Error);

            user_repo.Verify(r => r.GetUserByIdAsyncRepo(userId, It.IsAny<CancellationToken>()), Times.Once);
            user_repo.VerifyNoOtherCalls();

        }


        [Fact]
        public async Task GetMe_WhenUserNull_ReturnsFail()
        {
            var id = Guid.NewGuid();

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            user_repo.Setup(r => r.GetMeRepo(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserDTO?)null);

            var res = await sut.GetMe(id, CancellationToken.None);

            Assert.False(res.Success);
            Assert.Equal("User does not exist", res.Error);

            user_repo.Verify(r => r.GetMeRepo(id, It.IsAny<CancellationToken>()), Times.Once);
            user_repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetMe_WhenOk_ReturnsOk()
        {
            var id = Guid.NewGuid();
            var dto = new UserDTO();

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            user_repo.Setup(r => r.GetMeRepo(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var res = await sut.GetMe(id, CancellationToken.None);

            Assert.True(res.Success);
            Assert.Same(dto, res.Value);

            user_repo.Verify(r => r.GetMeRepo(id, It.IsAny<CancellationToken>()), Times.Once);
            user_repo.VerifyNoOtherCalls();
        }


        [Fact]
        public async Task LogoutUserAsync_WhenOk_ReturnsOkTrue()
        {
            
            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            password_serv.Setup(p => p.RevokeRefreshToken("REF", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var res = await sut.LogoutUserAsync("REF", CancellationToken.None);

            Assert.True(res.Success);
            Assert.True(res.Value);

            password_serv.Verify(p => p.RevokeRefreshToken("REF", It.IsAny<CancellationToken>()), Times.Once);
            password_serv.VerifyNoOtherCalls();
        }


        [Fact]
        public async Task IsUserAdmin_WhenOk_ReturnsOkWithValue()
        {
            var id = Guid.NewGuid();

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            user_repo.Setup(r => r.IsUserAdminRepo(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var res = await sut.IsUserAdmin(id, CancellationToken.None);

            Assert.True(res.Success);
            Assert.True(res.Value);

            user_repo.Verify(r => r.IsUserAdminRepo(id, It.IsAny<CancellationToken>()), Times.Once);
            user_repo.VerifyNoOtherCalls();
        }


        [Fact]
        public async Task ChangeUserLanguage_WhenValidationFails_ReturnsFail()
        {
            var dto = new ChangeLanguageDTO { Language = "xx" };

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut(language_validation: Invalid(new ValidationFailure("Language", "bad")));

            var res = await sut.ChangeUserLanguage(dto, Guid.NewGuid(), CancellationToken.None);

            Assert.False(res.Success);
            Assert.Equal("Incorrect Data Entry", res.Error);

            user_repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ChangeUserLanguage_WhenOk_ReturnsOk()
        {
            var dto = new ChangeLanguageDTO { Language = "en" };
            var userId = Guid.NewGuid();

            var (sut, password_serv, jwt_serv, user_repo, tokens_repo) = CreateSut();

            user_repo.Setup(r => r.ChangeUserLanguageRepo(dto.Language, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var res = await sut.ChangeUserLanguage(dto, userId, CancellationToken.None);

            Assert.True(res.Success);
            Assert.True(res.Value);

            user_repo.Verify(r => r.ChangeUserLanguageRepo(dto.Language, userId, It.IsAny<CancellationToken>()), Times.Once);
            user_repo.VerifyNoOtherCalls();
        }
    }
}
