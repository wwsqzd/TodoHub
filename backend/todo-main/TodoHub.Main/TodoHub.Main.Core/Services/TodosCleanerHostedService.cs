
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
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
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync("todo_queue", false, false, false, null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (ch, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var userId = Guid.Parse(message);
                using var scope = _scopeFactory.CreateScope();
                var todosService = scope.ServiceProvider.GetRequiredService<ITodosCleanerService>();
                // Вот здесь вызываем Core сервис
                await todosService.CleanALlTodosByUser(userId);
            };

            await channel.BasicConsumeAsync("todo_queue", true, consumer);

            // HostedService работает пока приложение запущено
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
