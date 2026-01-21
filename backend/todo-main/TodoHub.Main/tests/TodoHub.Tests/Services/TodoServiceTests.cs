using Elastic.Clients.Elasticsearch.Core.Search;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Interfaces;
using TodoHub.Main.Core.Services;

namespace TodoHub.Tests.Services
{
    public class TodoServiceTests
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


        private (TodoService sut, 
            Mock<ITodoRepository> repo,
            Mock<ITodoCacheService> cache,
            Mock<IElastikSearchService> es
            ) CreateSut(ValidationResult? createValidation = null,
        ValidationResult? updateValidation = null)
        {
            var repo = new Mock<ITodoRepository>(MockBehavior.Strict);
            var cache = new Mock<ITodoCacheService>(MockBehavior.Strict);
            var es = new Mock<IElastikSearchService>(MockBehavior.Strict);


            var createValidator = new FakeValidator<CreateTodoDTO>(createValidation ?? Valid());
            var updateValidator = new FakeValidator<UpdateTodoDTO>(updateValidation ?? Valid());

           
            var dbBulkhead = new DbBulkhead();
            var esBulkhead = new EsBulkhead();

            var sut = new TodoService(
                repo.Object,
                createValidator,
                updateValidator,
                cache.Object,
                es.Object,
                dbBulkhead,
                esBulkhead);

            return (sut, repo, cache, es);
        }



        // Validation Fail
        [Fact]
        public async Task AddTodoAsync_WhenValidationFails_ShouldReturnFail_AndNotCallDependencies()
        {
            // !!!Arrange!!!
            var todo = new CreateTodoDTO();
            var ownerId = Guid.NewGuid();


            var (sut, repo, cache, es) = CreateSut(
            createValidation: Invalid(new ValidationFailure("Title", "bad")));

            // !!!Act!!!
            var result = await sut.AddTodoAsync(todo, ownerId, CancellationToken.None);

            // !!!Assert!!
            Assert.False(result.Success);
            Assert.Equal("Incorrect data entry", result.Error);


            repo.VerifyNoOtherCalls();
            cache.VerifyNoOtherCalls();
            es.VerifyNoOtherCalls();
        }



        // Happy path
        [Fact]
        public async Task AddTodoAsync_HappyPath_ReturnsOk_AndCallsRepoEsCacheOnce()
        {
            // Arrange
            var input = new CreateTodoDTO();
            var ownerId = Guid.NewGuid();
            var created = new TodoDTO { Id = Guid.NewGuid()};

            var (sut, repo, cache, es) = CreateSut();

            // Setup 
            repo.Setup(r => r.AddTodoAsyncRepo(input, ownerId, It.IsAny<CancellationToken>())).ReturnsAsync(created);
            cache.Setup(s => s.DeleteCache(ownerId)).Returns(Task.CompletedTask);
            es.Setup(s => s.UpsertDoc(created, created.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Result<bool>.Ok(true));

           
            // Act
            var result = await sut.AddTodoAsync(input, ownerId, CancellationToken.None);

            // Assert

            Assert.True(result.Success);            
            Assert.Equal(created, result.Value);     

            repo.Verify(r => r.AddTodoAsyncRepo(input, ownerId, It.IsAny<CancellationToken>()), Times.Once);
            es.Verify(s => s.UpsertDoc(created, created.Id, It.IsAny<CancellationToken>()), Times.Once);
            cache.Verify(c => c.DeleteCache(ownerId), Times.Once);

            
            repo.VerifyNoOtherCalls();
            es.VerifyNoOtherCalls();
            cache.VerifyNoOtherCalls();

        }

        // Repo throws
        [Fact]
        public async Task AddTodoAsync_RepoThrows_ReturnsFail_AndDoesNotCallEsOrCache()
        {
            // Arrange
            var input = new CreateTodoDTO();
            var ownerId = Guid.NewGuid();

            var (sut, repo, cache, es) = CreateSut();


            repo.Setup(r => r.AddTodoAsyncRepo(input, ownerId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB is down"));

            

            // Act
            var result = await sut.AddTodoAsync(input, ownerId, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Exception:", result.Error);
            Assert.Contains("DB is down", result.Error);

            repo.Verify(r => r.AddTodoAsyncRepo(input, ownerId, It.IsAny<CancellationToken>()), Times.Once);

            repo.VerifyNoOtherCalls();
            es.VerifyNoOtherCalls();
            cache.VerifyNoOtherCalls();
        }

        // Es throws
        [Fact]
        public async Task AddTodoAsync_EsThrows_ReturnsFail_AndDoesNotCallCache()
        {
            // Arrange
            var input = new CreateTodoDTO();
            var created = new TodoDTO { Id = Guid.NewGuid()};
            var ownerId = Guid.NewGuid();

            var (sut, repo, cache, es) = CreateSut();

            repo.Setup(r => r.AddTodoAsyncRepo(input, ownerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

            es.Setup(s => s.UpsertDoc(created, created.Id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Es is down"));


            // Act
            var result = await sut.AddTodoAsync(input, ownerId, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Exception:", result.Error);
            Assert.Contains("Es is down", result.Error);

            repo.Verify(r => r.AddTodoAsyncRepo(input, ownerId, It.IsAny<CancellationToken>()), Times.Once);
            es.Verify(s => s.UpsertDoc(created, created.Id, It.IsAny<CancellationToken>()), Times.Once);

            repo.VerifyNoOtherCalls();
            es.VerifyNoOtherCalls();
            cache.VerifyNoOtherCalls();
        }


        // Cache throws
        [Fact]
        public async Task AddTodoAsync_CacheThrows_ReturnsFail()
        {
            // Arrange
            var input = new CreateTodoDTO();
            var created = new TodoDTO { Id = Guid.NewGuid() };
            var ownerId = Guid.NewGuid();

            var (sut, repo, cache, es) = CreateSut();

            repo.Setup(r => r.AddTodoAsyncRepo(input, ownerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

            es.Setup(s => s.UpsertDoc(created, created.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Ok(true));

            cache.Setup(s => s.DeleteCache(ownerId)).ThrowsAsync(new InvalidOperationException("Cache is down"));


            // Act
            var result = await sut.AddTodoAsync(input, ownerId, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Exception:", result.Error);
            Assert.Contains("Cache is down", result.Error);

            repo.Verify(r => r.AddTodoAsyncRepo(input, ownerId, It.IsAny<CancellationToken>()), Times.Once);
            es.Verify(s => s.UpsertDoc(created, created.Id, It.IsAny<CancellationToken>()), Times.Once);
            cache.Verify(s => s.DeleteCache(ownerId), Times.Once);

            repo.VerifyNoOtherCalls();
            es.VerifyNoOtherCalls();
            cache.VerifyNoOtherCalls();
        }

        // Happy Path
        [Fact]
        public async Task DeleteTodoAsync_HappyPath_ReturnsOk_AndCallsRepoEsCacheOnce()
        {
            // Arrange
            var todoId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var deletedId = Guid.NewGuid();


            var (sut, repo, cache, es) = CreateSut();

            repo.Setup(r => r.DeleteTodoAsyncRepo(todoId, ownerId, It.IsAny<CancellationToken>())).ReturnsAsync(deletedId);

            es.Setup(s => s.DeleteDoc(todoId, ownerId, It.IsAny<CancellationToken>())).ReturnsAsync(Result<bool>.Ok(true));

            cache.Setup(s => s.DeleteCache(ownerId)).Returns(Task.CompletedTask);


            // Act
            var result = await sut.DeleteTodoAsync(todoId, ownerId, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(deletedId, result.Value);

            repo.Verify(r => r.DeleteTodoAsyncRepo(todoId, ownerId, It.IsAny<CancellationToken>()), Times.Once);
            es.Verify(s => s.DeleteDoc(todoId, ownerId, It.IsAny<CancellationToken>()), Times.Once);
            cache.Verify(s => s.DeleteCache(ownerId), Times.Once);


            repo.VerifyNoOtherCalls();
            es.VerifyNoOtherCalls();
            cache.VerifyNoOtherCalls();
        }


        // Repo Throws
        [Fact]
        public async Task DeleteTodoAsync_RepoThrows_ReturnsFail_AndDoesNotCallEsOrCache()
        {
            // Arrange
            var todoId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var (sut, repo, cache, es) = CreateSut();

            repo.Setup(r => r.DeleteTodoAsyncRepo(todoId, ownerId, It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("DB is down"));


            // Act
            var result = await sut.DeleteTodoAsync(todoId, ownerId, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Exception", result.Error);
            Assert.Contains("DB is down", result.Error);


            repo.Verify(r => r.DeleteTodoAsyncRepo(todoId, ownerId, It.IsAny<CancellationToken>()), Times.Once);


            repo.VerifyNoOtherCalls();
            es.VerifyNoOtherCalls();
            cache.VerifyNoOtherCalls();
        }


        // Es Throws
        [Fact]
        public async Task DeleteTodoAsync_EsThrows_ReturnsFail_AndDoesNotCallCache()
        {
            // Arrange
            var todoId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var deletedId = Guid.NewGuid();

            var (sut, repo, cache, es) = CreateSut();

            repo.Setup(r => r.DeleteTodoAsyncRepo(todoId, ownerId, It.IsAny<CancellationToken>())).ReturnsAsync(deletedId);
            es.Setup(s => s.DeleteDoc(todoId, ownerId, It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Es is down"));



            // Act
            var result = await sut.DeleteTodoAsync(todoId, ownerId, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Exception", result.Error);
            Assert.Contains("Es is down", result.Error);


            repo.Verify(r => r.DeleteTodoAsyncRepo(todoId, ownerId, It.IsAny<CancellationToken>()), Times.Once);
            es.Verify(s => s.DeleteDoc(todoId, ownerId, It.IsAny<CancellationToken>()), Times.Once);

            repo.VerifyNoOtherCalls();
            es.VerifyNoOtherCalls();
            cache.VerifyNoOtherCalls();
        }



        // Cache Throws
        [Fact]
        public async Task DeleteTodoAsync_CacheThrows_ReturnsFail()
        {
            // Arrange
            var todoId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var deletedId = Guid.NewGuid();


            var (sut, repo, cache, es) = CreateSut();


            repo.Setup(r => r.DeleteTodoAsyncRepo(todoId, ownerId, It.IsAny<CancellationToken>())).ReturnsAsync(deletedId);

            es.Setup(s => s.DeleteDoc(todoId, ownerId, It.IsAny<CancellationToken>())).ReturnsAsync(Result<bool>.Ok(true));

            cache.Setup(s => s.DeleteCache(ownerId)).ThrowsAsync(new InvalidOperationException("Cache is down"));

            

            // Act
            var result = await sut.DeleteTodoAsync(todoId, ownerId, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Exception", result.Error);
            Assert.Contains("Cache is down", result.Error);


            repo.Verify(r => r.DeleteTodoAsyncRepo(todoId, ownerId, It.IsAny<CancellationToken>()), Times.Once);
            es.Verify(s => s.DeleteDoc(todoId, ownerId, It.IsAny<CancellationToken>()), Times.Once);
            cache.Verify(s => s.DeleteCache(ownerId), Times.Once);


            repo.VerifyNoOtherCalls();
            es.VerifyNoOtherCalls();
            cache.VerifyNoOtherCalls();
        }



        [Fact]
        public async Task GetTodoByIdAsync_HappyPath_ReturnsOk_AndCallsRepoJustOnce()
        {
            // Arrange
            var todoId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var todo = new TodoDTO();


            var (sut, repo, cache, es) = CreateSut();


            repo.Setup(r => r.GetTodoByIdAsyncRepo(todoId, ownerId, It.IsAny<CancellationToken>())).ReturnsAsync(todo);

            
            // Act
            var result = await sut.GetTodoByIdAsync(todoId, ownerId, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(todo, result.Value);


            repo.Verify(r => r.GetTodoByIdAsyncRepo(todoId, ownerId, It.IsAny<CancellationToken>()), Times.Once);

            repo.VerifyNoOtherCalls();
        }


        [Fact]
        public async Task GetTodoByIdAsync_RepoThrows_ReturnFail()
        {
            // Arrange
            var todoId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();


            var (sut, repo, cache, es) = CreateSut();

            repo.Setup(r => r.GetTodoByIdAsyncRepo(todoId, ownerId, It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Repo is down"));

            // Act
            var result = await sut.GetTodoByIdAsync(todoId, ownerId, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Exception", result.Error);
            Assert.Contains("Repo is down", result.Error);

            repo.Verify(r => r.GetTodoByIdAsyncRepo(todoId, ownerId, It.IsAny<CancellationToken>()), Times.Once);

            repo.VerifyNoOtherCalls();
            es.VerifyNoOtherCalls();
            cache.VerifyNoOtherCalls();
        }


        [Fact]
        public async Task GetTodosAsync_HappyPath_ReturnsOk_TodoCache_AndCallCacheJustOnce()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var todos_redis = new List<TodoDTO>
            {
                new TodoDTO { Id = Guid.NewGuid() }
            };

            var (sut, repo, cache, es) = CreateSut();

            cache.Setup(r => r.GetTodosAsync(userId)).ReturnsAsync(todos_redis);

            // Act
            var result = await sut.GetTodosAsync(userId, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(todos_redis, result.Value);

            cache.Verify(r => r.GetTodosAsync(userId), Times.Once);

            repo.VerifyNoOtherCalls();
            es.VerifyNoOtherCalls();
            cache.VerifyNoOtherCalls();
        }


        [Fact]
        public async Task GetTodosAsync_HappyPath_ReturnsOk_TodoRepo_AndCallCacheAndRepoJustOnce()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var todos_redis = new List<TodoDTO>();
            var todos_repo = new List<TodoDTO>
            {
                new TodoDTO {Id = userId}
            };
            var todos_for_cache = new List<TodoDTO>
            {
                new TodoDTO {Id = userId}
            };

            var (sut, repo, cache, es) = CreateSut();

            cache.Setup(r => r.GetTodosAsync(userId)).ReturnsAsync(todos_redis);
            repo.Setup(s => s.GetTodosAsyncRepo(userId, It.IsAny<CancellationToken>())).ReturnsAsync(todos_for_cache);
            cache.Setup(s => s.SetTodosAsync(todos_for_cache, userId)).Returns(Task.CompletedTask);

            

            // Act
            var result = await sut.GetTodosAsync(userId, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(todos_for_cache, result.Value);

            cache.Verify(c => c.GetTodosAsync(userId), Times.Once);
            repo.Verify(r => r.GetTodosAsyncRepo(userId, It.IsAny<CancellationToken>()), Times.Once);
            cache.Verify(c => c.SetTodosAsync(todos_for_cache, userId), Times.Once);

            repo.VerifyNoOtherCalls();
            es.VerifyNoOtherCalls();
            cache.VerifyNoOtherCalls();
        }


        // Validation Fail
        [Fact]
        public async Task UpdateTodoAsync_WhenValidationFails_ShouldReturnFail_AndNotCallDependencies()
        {
            // !!!Arrange!!!
            var todo = new UpdateTodoDTO();
            var OwnerId = Guid.NewGuid();
            var TodoId = Guid.NewGuid();

            var (sut, repo, cache, es) = CreateSut(
           updateValidation: Invalid(new ValidationFailure("Description", "Bad")));
            

            // Act
            var result = await sut.UpdateTodoAsync(todo, OwnerId, TodoId, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Incorrect data entry", result.Error);


            repo.VerifyNoOtherCalls();
            cache.VerifyNoOtherCalls();
            es.VerifyNoOtherCalls();
        }



        // Happy Path
        [Fact]
        public async Task UpdateTodoAsync_HappyPath_ReturnOk_AndCallAllJustOnce()
        {
            // !!!Arrange!!!
            var todo = new UpdateTodoDTO();
            var ownerId = Guid.NewGuid();
            var todoId = Guid.NewGuid();

            var updatedTodo = new TodoDTO { Id = todoId };

            var (sut, repo, cache, es) = CreateSut();


            repo.Setup(r => r.UpdateTodoAsyncRepo(todo, ownerId, todoId, It.IsAny<CancellationToken>())).ReturnsAsync(updatedTodo);
            es.Setup(s => s.UpsertDoc(updatedTodo, todoId, It.IsAny<CancellationToken>())).ReturnsAsync(Result<bool>.Ok(true));
            cache.Setup(c => c.DeleteCache(ownerId)).Returns(Task.CompletedTask);


            // Act
            var result = await sut.UpdateTodoAsync(todo, ownerId, todoId, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(updatedTodo, result.Value);

            repo.Verify(r => r.UpdateTodoAsyncRepo(todo, ownerId, todoId, It.IsAny<CancellationToken>()), Times.Once);
            es.Verify(r => r.UpsertDoc(updatedTodo, todoId, It.IsAny<CancellationToken>()), Times.Once);
            cache.Verify(r => r.DeleteCache(ownerId), Times.Once);


            repo.VerifyNoOtherCalls();
            cache.VerifyNoOtherCalls();
            es.VerifyNoOtherCalls();  
        }



        // Repo throws
        [Fact]
        public async Task UpdateTodoAsync_RepoThrows_ReturnFail_AndDoesNotCallAnything()
        {
            // !!!Arrange!!!
            var todo = new UpdateTodoDTO();
            var ownerId = Guid.NewGuid();
            var todoId = Guid.NewGuid();

            var updatedTodo = new TodoDTO { Id = todoId };

            var (sut, repo, cache, es) = CreateSut();

            repo.Setup(r => r.UpdateTodoAsyncRepo(todo, ownerId, todoId, It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("DB is down"));

            // Act
            var result = await sut.UpdateTodoAsync(todo, ownerId, todoId, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Exception", result.Error);
            Assert.Contains("DB is down", result.Error);

            repo.Verify(r => r.UpdateTodoAsyncRepo(todo, ownerId, todoId, It.IsAny<CancellationToken>()), Times.Once);


            repo.VerifyNoOtherCalls();
            cache.VerifyNoOtherCalls();
            es.VerifyNoOtherCalls();
        }



        // Es Throws
        [Fact]
        public async Task UpdateTodoAsync_EsThrows_ReturnFail_AndDoNotCallCache()
        {
            // !!!Arrange!!!
            var todo = new UpdateTodoDTO();
            var ownerId = Guid.NewGuid();
            var todoId = Guid.NewGuid();

            var updatedTodo = new TodoDTO { Id = todoId };

            var (sut, repo, cache, es) = CreateSut();


            repo.Setup(r => r.UpdateTodoAsyncRepo(todo, ownerId, todoId, It.IsAny<CancellationToken>())).ReturnsAsync(updatedTodo);
            es.Setup(s => s.UpsertDoc(updatedTodo, todoId, It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Es is down"));


            // Act
            var result = await sut.UpdateTodoAsync(todo, ownerId, todoId, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Exception", result.Error);
            Assert.Contains("Es is down", result.Error);

            repo.Verify(r => r.UpdateTodoAsyncRepo(todo, ownerId, todoId, It.IsAny<CancellationToken>()), Times.Once);
            es.Verify(r => r.UpsertDoc(updatedTodo, todoId, It.IsAny<CancellationToken>()), Times.Once);


            repo.VerifyNoOtherCalls();
            cache.VerifyNoOtherCalls();
            es.VerifyNoOtherCalls();
        }


        // Cache Throws
        [Fact]
        public async Task UpdateTodoAsync_CacheThrows_ReturnFail()
        {
            // !!!Arrange!!!
            var todo = new UpdateTodoDTO();
            var ownerId = Guid.NewGuid();
            var todoId = Guid.NewGuid();

            var updatedTodo = new TodoDTO { Id = todoId };

            var (sut, repo, cache, es) = CreateSut();

            repo.Setup(r => r.UpdateTodoAsyncRepo(todo, ownerId, todoId, It.IsAny<CancellationToken>())).ReturnsAsync(updatedTodo);
            es.Setup(s => s.UpsertDoc(updatedTodo, todoId, It.IsAny<CancellationToken>())).ReturnsAsync(Result<bool>.Ok(true));
            cache.Setup(c => c.DeleteCache(ownerId)).ThrowsAsync(new InvalidOperationException("Cache is down"));


            // Act
            var result = await sut.UpdateTodoAsync(todo, ownerId, todoId, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Exception", result.Error);
            Assert.Contains("Cache is down", result.Error);

            repo.Verify(r => r.UpdateTodoAsyncRepo(todo, ownerId, todoId, It.IsAny<CancellationToken>()), Times.Once);
            es.Verify(r => r.UpsertDoc(updatedTodo, todoId, It.IsAny<CancellationToken>()), Times.Once);
            cache.Verify(r => r.DeleteCache(ownerId), Times.Once);


            repo.VerifyNoOtherCalls();
            cache.VerifyNoOtherCalls();
            es.VerifyNoOtherCalls();
        }
    }
}
