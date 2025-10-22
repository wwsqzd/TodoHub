using Microsoft.EntityFrameworkCore;
using TodoHub.Main.Core.Entities;

namespace TodoHub.Main.DataAccess.Context
{
    // Context for the database 
    public class ApplicationDbContext : DbContext
    {
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<TodoEntity> Todos { get; set; }
        public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }
        public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options) : base (options)
        {
            // Promises that the database will be created
            Database.EnsureCreated();
        }
    }
}
