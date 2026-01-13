
using FluentValidation;
using FluentValidation.Results;
using Serilog;
using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.Core.Services
{
    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _todoRepository;
        private readonly AbstractValidator<CreateTodoDTO> _createvalidator;
        private readonly AbstractValidator<UpdateTodoDTO> _updatevalidator;
        private readonly ITodoCacheService _todoCacheService;
        private readonly IElastikSearchService _elastikSearchService;
        private readonly DbBulkhead _dbBulkhead;
        private readonly EsBulkhead _esBulkhead;
        

        public TodoService(
            ITodoRepository todoRepository,
            AbstractValidator<CreateTodoDTO> create_validator,
            AbstractValidator<UpdateTodoDTO> updatevalidator,
            ITodoCacheService todoCacheService,
            IElastikSearchService elastikSearchService,
            DbBulkhead dbBulkhead,
            EsBulkhead esBulkhead
            
            )
        {
            _todoRepository = todoRepository;
            _createvalidator = create_validator;
            _updatevalidator = updatevalidator;
            _todoCacheService = todoCacheService;
            _elastikSearchService = elastikSearchService;
            _dbBulkhead = dbBulkhead;
            _esBulkhead = esBulkhead;
            
        }
        // add todo 
        public async Task<Result<TodoDTO>> AddTodoAsync(CreateTodoDTO todo, Guid OwnerId, CancellationToken ct)
        {
            Log.Information("AddTodoAsync starting in TodoService");

            ValidationResult res = _createvalidator.Validate(todo);
            if (!res.IsValid)
            {
                return Result<TodoDTO>.Fail("Incorrect data entry");
            }

            try
            {
                var createdTodo = await _dbBulkhead.ExecuteAsync(bulkCt => ResilienceExecutor.WithTimeout(t => _todoRepository.AddTodoAsyncRepo(todo, OwnerId, t),TimeSpan.FromSeconds(5),bulkCt),ct);
                var elastik_response = await _esBulkhead.ExecuteAsync(bulkCt => ResilienceExecutor.WithTimeout(t => _elastikSearchService.UpsertDoc(createdTodo, createdTodo.Id, t), TimeSpan.FromSeconds(5), bulkCt),ct);

                if (!elastik_response.Success)
                {
                    return Result<TodoDTO>.Fail(elastik_response.Error);
                }
                await _todoCacheService.DeleteCache(OwnerId);
                return Result<TodoDTO>.Ok(createdTodo);
            } catch (BulkheadRejektedException ex)
            {
                return Result<TodoDTO>.Fail($"Overloaded: {ex.Message}");
            }
        }

        // delete todo
        public async Task<Result<Guid>> DeleteTodoAsync(Guid id, Guid OwnerId, CancellationToken ct)
        {
            try
            {
                var res = await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => _todoRepository.DeleteTodoAsyncRepo(id, OwnerId, t), TimeSpan.FromSeconds(5), bct), ct);
                if (res == null)
                {
                    return Result<Guid>.Fail("Error deleting todo");
                }
                await _esBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => _elastikSearchService.DeleteDoc(id, OwnerId, t), TimeSpan.FromSeconds(5), bct), ct);
                await _todoCacheService.DeleteCache(OwnerId);
                return Result<Guid>.Ok(res.Value);
            } catch (BulkheadRejektedException ex)
            {
                return Result<Guid>.Fail($"Overloaded: {ex.Message}");
            }
        }

        // get todo by id
        public async Task<Result<TodoDTO>> GetTodoByIdAsync(Guid id, Guid OwnerId, CancellationToken ct)
        {
            try
            {
                var todo = await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => _todoRepository.GetTodoByIdAsyncRepo(id, OwnerId, t), TimeSpan.FromSeconds(2), bct), ct);
                if (todo == null)
                {
                    return Result<TodoDTO>.Fail("Todo does not exist");
                }
                return Result<TodoDTO>.Ok(todo);
            } catch (BulkheadRejektedException ex)
            {
                return Result<TodoDTO>.Fail($"Overloaded: {ex.Message}");
            }
        }

        // get todos
        public async Task<Result<List<TodoDTO>>> GetTodosAsync(Guid UserId, DateTime? lastCreated, Guid? lastId, CancellationToken ct)
        {
            try
            {
                var todos_redis = await _todoCacheService.GetTodosAsync(UserId, lastCreated, lastId);
                if (todos_redis.Any() == true)
                {
                    return Result<List<TodoDTO>>.Ok(todos_redis, "Todos from redis");
                }
                var todosPage = await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => _todoRepository.GetTodosByPageAsyncRepo(UserId, lastCreated, lastId, t), TimeSpan.FromSeconds(2), bct), ct);
                if (todosPage == null)
                {
                    return Result<List<TodoDTO>>.Fail("Todos is empty in Database");
                }
                var todos = await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => _todoRepository.GetTodosAsyncRepo(UserId, t), TimeSpan.FromSeconds(2), bct), ct);
                await _todoCacheService.SetTodosAsync(todos, UserId);
                return Result<List<TodoDTO>>.Ok(todosPage, "Todos from db");
            } catch (BulkheadRejektedException ex)
            {
                return Result<List<TodoDTO>>.Fail($"Overloaded: {ex.Message}");
            }
        }
        // update todo
        public async Task<Result<TodoDTO>> UpdateTodoAsync(UpdateTodoDTO todo, Guid OwnerId, Guid TodoId, CancellationToken ct)
        {
            try
            {
                ValidationResult res = _updatevalidator.Validate(todo);
                if (!res.IsValid)
                {
                    return Result<TodoDTO>.Fail("Incorrect data entry");
                }
                var updatedTodo = await _dbBulkhead.ExecuteAsync(bct => _todoRepository.UpdateTodoAsyncRepo(todo, OwnerId, TodoId, bct), ct);
                await _esBulkhead.ExecuteAsync(bct => _elastikSearchService.UpsertDoc(updatedTodo, updatedTodo.Id, bct), ct);
                await _todoCacheService.DeleteCache(OwnerId);
                return Result<TodoDTO>.Ok(updatedTodo);
            } catch (BulkheadRejektedException ex)
            {
                return Result<TodoDTO>.Fail($"Overloaded: {ex.Message}");
            }
        }
    }
}
