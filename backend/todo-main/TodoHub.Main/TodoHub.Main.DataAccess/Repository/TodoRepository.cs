// Todo Repository

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Entities;
using TodoHub.Main.Core.Interfaces;
using TodoHub.Main.DataAccess.Context;

namespace TodoHub.Main.DataAccess.Repository
{
    // Repository for Todo
    public class TodoRepository : ITodoRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TodoRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Adding a todo 
        public async Task<TodoDTO> AddTodoAsyncRepo(CreateTodoDTO todo, Guid OwnerId)
        {
            var entity = _mapper.Map<TodoEntity>(todo);
            entity.OwnerId = OwnerId;
            await _context.Todos.AddAsync(entity);
            await _context.SaveChangesAsync();
            var output = _mapper.Map<TodoDTO>(entity);
            return output;
        }

        // Deleting todo
        public async Task<Guid?> DeleteTodoAsyncRepo(Guid id, Guid OwnerId)
        {
            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == OwnerId);
            if (todo == null) { return null; }

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();
            return todo.Id;
        }

        //Deleting all todo in deleted user
        public async Task<bool> DeleteAllTodoByUserAsyncRepo(Guid OwnerId)
        {
            var todos = await _context.Todos.Where(t => t.OwnerId == OwnerId).ToListAsync();
            _context.Todos.RemoveRange(todos);
            await _context.SaveChangesAsync();            
            return true;
        }

        // Get one todo by ID
        public async Task<TodoDTO?> GetTodoByIdAsyncRepo(Guid id, Guid OwnerId)
        {
            var todos = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == OwnerId);
            var todosDTOs = _mapper.Map<TodoDTO>(todos);
            return todosDTOs;
        }

        // Get all todo by ID
        public async Task<List<TodoDTO>> GetTodosAsyncRepo(Guid UserId)
        {
            var todos = await _context.Todos.Where(t => t.OwnerId == UserId).OrderBy(t => t.IsCompleted).ThenByDescending(t => t.CreatedDate).ToListAsync();
            var todoDTOs = _mapper.Map<List<TodoDTO>>(todos);
            return todoDTOs;

        }

        // Update todo
        public async Task<TodoDTO> UpdateTodoAsyncRepo(UpdateTodoDTO todo_to_update, Guid OwnerId, Guid TodoId)
        {
            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == TodoId && t.OwnerId == OwnerId);

            if (todo == null)
                throw new Exception("Todo not found or you don't have access.");

            if (!string.IsNullOrEmpty(todo_to_update.Title))
                todo.Title = todo_to_update.Title;

            if (!string.IsNullOrEmpty(todo_to_update.Description))
                todo.Description = todo_to_update.Description;

            if (todo_to_update.IsCompleted.HasValue)
                todo.IsCompleted = todo_to_update.IsCompleted.Value;

            todo.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            var output = _mapper.Map<TodoDTO>(todo);
            return output;
        }
    }
}
