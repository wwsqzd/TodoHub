

using Moq;
using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.Services;
using TodoHub.Main.DataAccess.Interfaces;

namespace TodoHub.Tests.Services
{
    public class RefreshTokensCleanerServiceTests
    {
        [Fact]
        public async Task CleanAllRefreshTokens_HappyPath_ReturnOk_AndCallOtherJustOnce()
        {
            // Arrange
            var repo = new Mock<IRefreshTokenRepository>();
            var dbBulkHead = new DbBulkhead();

            repo.Setup(r => r.DeleteOldTokensRepo(It.IsAny<CancellationToken>())).ReturnsAsync(true);


            var sut = new RefreshTokensCleanerService(repo.Object, dbBulkHead);

            // Act
            var response = await sut.CleanAllRefreshTokens(CancellationToken.None);

            // Assert
            Assert.True(response.Success);

            repo.Verify(r => r.DeleteOldTokensRepo(It.IsAny<CancellationToken>()), Times.Once);

            repo.VerifyNoOtherCalls();
        }


        [Fact]
        public async Task CleanAllRefreshTokens_RepoThrows_ReturnFail()
        {
            // Arrange
            var repo = new Mock<IRefreshTokenRepository>();
            var dbBulkHead = new DbBulkhead();

            repo.Setup(r => r.DeleteOldTokensRepo(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("DB is down"));


            var sut = new RefreshTokensCleanerService(repo.Object, dbBulkHead);

            // Act
            var response = await sut.CleanAllRefreshTokens(CancellationToken.None);

            // Assert
            Assert.False(response.Success);
            Assert.Equal("DB is down", response.Error);

            repo.Verify(r => r.DeleteOldTokensRepo(It.IsAny<CancellationToken>()), Times.Once);

            repo.VerifyNoOtherCalls();
        }
    }
}
