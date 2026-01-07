using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.Core.Services
{
    public class TodosCleanerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public TodosCleanerHostedService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Create a RabbitMQ connection factory pointing to localhost
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync("todo_queue", false, false, false, null);  // Declare a queue named "todo_queue"

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (ch, ea) => // Event triggered when a new message arrives in the queue
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var userId = Guid.Parse(message); // Parse the user ID from the message
                using var scope = _scopeFactory.CreateScope(); // Create a new service scope to get scoped services
                var todosService = scope.ServiceProvider.GetRequiredService<ITodosCleanerService>();
                // Service
                await ResilienceExecutor.WithTimeout(t => todosService.CleanALlTodosByUser(userId, t), TimeSpan.FromSeconds(5), stoppingToken);
            };

            await channel.BasicConsumeAsync("todo_queue", true, consumer);

            // HostedService runs while the application is running :)
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
