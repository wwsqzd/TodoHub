

using FluentValidation;
using FluentValidation.Results;
using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Entities;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.Core.Services
{
    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _todoRepository;
        private readonly AbstractValidator<CreateTodoDTO> _createvalidator;
        private readonly AbstractValidator<UpdateTodoDTO> _updatevalidator;

        public TodoService(ITodoRepository todoRepository, AbstractValidator<CreateTodoDTO> create_validator, AbstractValidator<UpdateTodoDTO> updatevalidator)
        {
            _todoRepository = todoRepository;
            _createvalidator = create_validator;
            _updatevalidator = updatevalidator;
        }
        public async Task<Result<CreateTodoDTO>> AddTodoAsync(CreateTodoDTO todo, Guid OwnerId)
        {
            ValidationResult res = _createvalidator.Validate(todo);
            if (!res.IsValid)
            {
                return Result<CreateTodoDTO>.Fail("incorrect data entry");
            }
            await _todoRepository.AddTodoAsyncRepo(todo, OwnerId);
            return Result<CreateTodoDTO>.Ok(todo);
        }

        public async Task<Result<bool>> DeleteTodoAsync(Guid id, Guid OwnerId)
        {
            await _todoRepository.DeleteTodoAsyncRepo(id, OwnerId);
            return Result<bool>.Ok(true);
        }

        public async Task<Result<TodoEntity>> GetTodoByIdAsync(Guid id, Guid OwnerId)
        {
            var todo = await _todoRepository.GetTodoByIdAsyncRepo(id, OwnerId);
            if (todo == null)
            {
                return Result<TodoEntity>.Fail("Todo does not exist");
            }
            return Result<TodoEntity>.Ok(todo);
        }

        public async Task<Result<List<TodoDTO>>> GetTodosAsync(Guid UserId)
        {
            var todos = await _todoRepository.GetTodosAsyncRepo(UserId);
            if (todos == null)
            {
                return Result<List<TodoDTO>>.Fail("null");
            }
            return Result<List<TodoDTO>>.Ok(todos);
        }

        public async Task<Result<UpdateTodoDTO>> UpdateTodoAsync(UpdateTodoDTO todo, Guid OwnerId, Guid TodoId)
        {
            ValidationResult res = _updatevalidator.Validate(todo);
            if (!res.IsValid)
            {
                return Result<UpdateTodoDTO>.Fail("Incorrect data entry");
            }
            await _todoRepository.UpdateTodoAsyncRepo(todo, OwnerId, TodoId);
            return Result<UpdateTodoDTO>.Ok(todo);
        }
    }
}
