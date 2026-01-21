// Todo Repository

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Serilog;
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
        public async Task<TodoDTO> AddTodoAsyncRepo(CreateTodoDTO todo, Guid OwnerId, CancellationToken ct)
        {
            Log.Information("AddTodoAsyncRepo starting in TodoRepository");
            var entity = _mapper.Map<TodoEntity>(todo);
            entity.OwnerId = OwnerId;
            await _context.Todos.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
            var output = _mapper.Map<TodoDTO>(entity);
            return output;
        }

        // Deleting todo
        public async Task<Guid?> DeleteTodoAsyncRepo(Guid id, Guid OwnerId, CancellationToken ct)
        {
            Log.Information("DeleteTodoAsyncRepo starting in TodoRepository");

            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == OwnerId, ct);
            if (todo == null) { return null; }

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync(ct);
            return todo.Id;
        }

        //Deleting all todo in deleted user
        public async Task<bool> DeleteAllTodoByUserAsyncRepo(Guid OwnerId, CancellationToken ct)
        {
            Log.Information("DeleteAllTodoByUserAsyncRepo starting in TodoRepository");

            var todos = await _context.Todos.Where(t => t.OwnerId == OwnerId).ToListAsync(ct);
            _context.Todos.RemoveRange(todos);
            await _context.SaveChangesAsync(ct);            
            return true;
        }

        // Get one todo by ID
        public async Task<TodoDTO?> GetTodoByIdAsyncRepo(Guid id, Guid OwnerId, CancellationToken ct)
        {
            Log.Information("GetTodoByIdAsyncRepo starting in TodoRepository");

            var todos = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == OwnerId,ct);
            var todosDTOs = _mapper.Map<TodoDTO>(todos);
            return todosDTOs;
        }

        // Get todo by userID and page
        //public async Task<List<TodoDTO>> GetTodosByPageAsyncRepo(Guid UserId, DateTime? lastCreated, Guid? lastId, CancellationToken ct)
        //{
        //    Log.Information("GetTodosByPageAsyncRepo starting in TodoRepository");

        //    var query = _context.Todos
        //        .Where(t => t.OwnerId == UserId);

        //    if (lastCreated != null && lastId != null)
        //    {
        //        query = query.Where(t =>
        //            t.CreatedDate < lastCreated.Value ||
        //            (t.CreatedDate == lastCreated.Value && t.Id.CompareTo(lastId.Value) < 0));
        //    }

        //    query = query.OrderBy(t => t.IsCompleted)
        //                 .ThenByDescending(t => t.CreatedDate)
        //                 .ThenByDescending(t => t.Id);

            
        //    var todos = await query.Take(10).ToListAsync(ct);

        //    var todoDTOs = _mapper.Map<List<TodoDTO>>(todos);
        //    return todoDTOs;
        //}

        // Get Todos
        public async Task<List<TodoDTO>> GetTodosAsyncRepo(Guid UserId, CancellationToken ct)
        {
            Log.Information("GetTodosAsyncRepo starting in TodoRepository");

            var todos = await _context.Todos
                .Where(t => t.OwnerId == UserId)
                .OrderBy(t => t.IsCompleted)
                .ThenByDescending(t => t.CreatedDate)
                .ThenByDescending(t => t.Id)
                .ToListAsync(ct);
            var todoDTOs = _mapper.Map<List<TodoDTO>>(todos);
            return todoDTOs;
        }

        // Update todo
        public async Task<TodoDTO> UpdateTodoAsyncRepo(UpdateTodoDTO todo_to_update, Guid OwnerId, Guid TodoId, CancellationToken ct)
        {
            Log.Information("UpdateTodoAsyncRepo starting in TodoRepository");

            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == TodoId && t.OwnerId == OwnerId, ct) ?? throw new Exception("Todo not found or you don't have access.");

            if (!string.IsNullOrEmpty(todo_to_update.Title))
                todo.Title = todo_to_update.Title;

            if (!string.IsNullOrEmpty(todo_to_update.Description))
                todo.Description = todo_to_update.Description;

            if (todo_to_update.IsCompleted.HasValue)
            {
                todo.IsCompleted = todo_to_update.IsCompleted.Value;
                if (todo_to_update.IsCompleted.Value ==  true)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == todo.OwnerId, ct);
                    
                    if (user != null)
                    {
                        var completionTime = (DateTime.UtcNow - todo.CreatedDate).TotalHours;
                        user.Complated_Todo++;
                        user.Average_completion_time = ((user.Average_completion_time * (user.Complated_Todo - 1)) + completionTime) / user.Complated_Todo;
                    }
                }
            }   

            todo.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            var output = _mapper.Map<TodoDTO>(todo);
            return output;
        }
    }
}
