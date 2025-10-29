
using FluentValidation;
using FluentValidation.Results;
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

        public TodoService(ITodoRepository todoRepository, AbstractValidator<CreateTodoDTO> create_validator, AbstractValidator<UpdateTodoDTO> updatevalidator, ITodoCacheService todoCacheService)
        {
            _todoRepository = todoRepository;
            _createvalidator = create_validator;
            _updatevalidator = updatevalidator;
            _todoCacheService = todoCacheService;
        }
        // add todo 
        public async Task<Result<TodoDTO>> AddTodoAsync(CreateTodoDTO todo, Guid OwnerId)
        {
            ValidationResult res = _createvalidator.Validate(todo);
            if (!res.IsValid)
            {
                return Result<TodoDTO>.Fail("Incorrect data entry");
            }
            var createdTodo = await _todoRepository.AddTodoAsyncRepo(todo, OwnerId);
            await _todoCacheService.DeleteCache(OwnerId);
            return Result<TodoDTO>.Ok(createdTodo);
        }

        // delete todo
        public async Task<Result<Guid>> DeleteTodoAsync(Guid id, Guid OwnerId)
        {
            var res = await _todoRepository.DeleteTodoAsyncRepo(id, OwnerId);
            if (res == null)
            {
                return Result<Guid>.Fail("Error deleting todo");
            }
            await _todoCacheService.DeleteCache(OwnerId);
            return Result<Guid>.Ok(res.Value);
        }

        // get todo by id
        public async Task<Result<TodoDTO>> GetTodoByIdAsync(Guid id, Guid OwnerId)
        {
            var todo = await _todoRepository.GetTodoByIdAsyncRepo(id, OwnerId);
            if (todo == null)
            {
                return Result<TodoDTO>.Fail("Todo does not exist");
            }
            return Result<TodoDTO>.Ok(todo);
        }

        // get todos
        public async Task<Result<List<TodoDTO>>> GetTodosAsync(Guid UserId, DateTime? lastCreated, Guid? lastId)
        {
            var todos_redis = await _todoCacheService.GetTodosAsync(UserId, lastCreated, lastId);
            if (todos_redis?.Any() == true)
            {
                return Result<List<TodoDTO>>.Ok(todos_redis, "Todos from redis");
            }
            var todosPage = await _todoRepository.GetTodosByPageAsyncRepo(UserId, lastCreated, lastId);
            if (todosPage == null)
            {
                return Result<List<TodoDTO>>.Fail("Todos is empty in Database");
            }
            var todos = await _todoRepository.GetTodosAsyncRepo(UserId);
            await _todoCacheService.SetTodosAsync(todos, UserId);
            return Result<List<TodoDTO>>.Ok(todosPage, "Todos from db");
        }
        // update todo
        public async Task<Result<TodoDTO>> UpdateTodoAsync(UpdateTodoDTO todo, Guid OwnerId, Guid TodoId)
        {
            ValidationResult res = _updatevalidator.Validate(todo);
            if (!res.IsValid)
            {
                return Result<TodoDTO>.Fail("Incorrect data entry");
            }
            var updatedTodo = await _todoRepository.UpdateTodoAsyncRepo(todo, OwnerId, TodoId);
            await _todoCacheService.DeleteCache(OwnerId);
            return Result<TodoDTO>.Ok(updatedTodo);
        }
    }
}
