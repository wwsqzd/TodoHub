using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TodoHub.Main.DataAccess.Context
{

    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] arg)
        {
            var config = new ConfigurationBuilder()
               .AddUserSecrets<ApplicationDbContextFactory>() // reads all keys from secrets
               //.AddJsonFile("appsettings.json", optional: true) 
               .Build();

            string? connectionString = config.GetConnectionString("DefaultConnection");


            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
