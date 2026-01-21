using Moq;
using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.Interfaces;
using TodoHub.Main.Core.Services;


namespace TodoHub.Tests.Services
{
    public class TodosCleanerServiceTests
    {
        [Fact]
        public async Task CleanALlTodosByUser_HappyPath_ReturnOk_AndCallAllJustOnce()
        {
            // Arrange
            var ownerId = Guid.NewGuid();

            var repo = new Mock<ITodoRepository>();

            repo.Setup(r => r.DeleteAllTodoByUserAsyncRepo(ownerId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var dbBulkhead = new DbBulkhead(); 

            var sut = new TodosCleanerService(
                repo.Object,
                dbBulkhead
            );

            // Act

            var response = await sut.CleanALlTodosByUser(ownerId, CancellationToken.None);

            //Assert

            Assert.True(response.Success);

            repo.Verify(r => r.DeleteAllTodoByUserAsyncRepo(ownerId, It.IsAny<CancellationToken>()), Times.Once);

            repo.VerifyNoOtherCalls();
        }


        [Fact]
        public async Task CleanALlTodosByUser_RepoThrows_ReturnFail()
        {
            // Arrange
            var ownerId = Guid.NewGuid();

            var repo = new Mock<ITodoRepository>();

            repo.Setup(r => r.DeleteAllTodoByUserAsyncRepo(ownerId, It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("DB is down"));

            var dbBulkhead = new DbBulkhead();

            var sut = new TodosCleanerService(
                repo.Object,
                dbBulkhead
            );

            // Act

            var response = await sut.CleanALlTodosByUser(ownerId, CancellationToken.None);

            //Assert

            Assert.False(response.Success);
            Assert.Equal("DB is down", response.Error);

            repo.Verify(r => r.DeleteAllTodoByUserAsyncRepo(ownerId, It.IsAny<CancellationToken>()), Times.Once);

            repo.VerifyNoOtherCalls();
        }
    }
}
