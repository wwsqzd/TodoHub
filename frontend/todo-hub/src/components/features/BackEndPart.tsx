import { Prism as SyntaxHighlighter } from "react-syntax-highlighter";
import { okaidia } from "react-syntax-highlighter/dist/esm/styles/prism";

export default function BackEndPart() {
  return (
    <div className="w-[1000px] flex gap-5 flex-col p-10">
      <p className="text-3xl ">Back-End</p>
      <div className="flex flex-col gap-2  p-5 rounded">
        <p className="text-lg">
          The back-end is organized into three main parts:
        </p>

        <ol className="p-2">
          <li>
            <strong>TodoHub.Main.API</strong> – handles HTTP requests and
            responses. It provides endpoints for the front-end to interact with
            the application, including user authentication, authorization and
            todo management.
          </li>
          <li>
            <strong>TodoHub.Main.Core</strong> – contains the core business
            logic and entity definitions. This layer defines models like
            UserEntity, TodoEntity, and RefreshTokenEntity, as well as
            interfaces and services that implement the application’s main
            functionality.
          </li>
          <li>
            <strong>TodoHub.Main.DataAccess</strong> – responsible for database
            interactions. It includes the ApplicationDbContext, entity
            configurations, and repositories such as UserRepository,
            TodoRepository, and RefreshTokenRepository to perform CRUD
            operations.
          </li>
        </ol>
      </div>

      <div className="flex flex-col gap-2  p-5 rounded">
        <p className="text-lg">
          The application provides several core technical features:
        </p>

        <ul className="p-2 gap-2">
          <li>
            <strong>User Authentication:</strong> Includes user registration and
            login with JWT-based access tokens and
            <em>HttpOnly</em> refresh tokens. The refresh token automatically
            renews the access token every 15 minutes for continuous secure
            access.
          </li>

          <li>
            <strong>Todo Management (CRUD):</strong> Users can create, read,
            update, and delete their todos. Each operation is securely bound to
            the authenticated user.
          </li>

          <li>
            <strong>Admin Panel:</strong> A dedicated panel for administrators
            with the ability to manage users, including deleting accounts when
            necessary.
          </li>

          <li>
            <strong>Asynchronous Deletion via RabbitMQ:</strong> When a user
            account is deleted, a background RabbitMQ worker is triggered to
            remove all related todos asynchronously, ensuring system
            responsiveness.
          </li>
          <li>
            <strong>Refresh Token Cleanup:</strong> A scheduled background task
            runs every 12 hours to remove expired or outdated refresh tokens,
            keeping the authentication system clean and efficient.
          </li>
          <li>
            <strong>Redis Caching:</strong> Redis is used to cache frequently
            accessed todos, allowing faster load times and reduced database
            requests.
          </li>
        </ul>
      </div>
      <div className="flex flex-col gap-5  p-5 rounded">
        <p className="text-lg font-bold">Implementation of RabbitMQ</p>
        <p>Extended documentation:</p>
        <p className="p-2">
          <a
            href="https://www.rabbitmq.com/tutorials/tutorial-two-dotnet#where-to-get-help"
            target="_blank"
            rel="noopener noreferrer"
            className="hover:text-blue-400 font-bold hover:underline"
          >
            RabbitMQ tutorial - Work Queues
          </a>
        </p>

        <p>
          Producer: <br /> A Producer sends messages to a message queue (like
          RabbitMQ), encapsulating data or commands for other services.
        </p>
        <SyntaxHighlighter
          language="csharp"
          style={okaidia}
          customStyle={{
            maxHeight: "300px",
            overflowY: "auto",
            fontSize: "14px",
            padding: "16px",
            borderRadius: "8px",
          }}
        >
          {`using RabbitMQ.Client;
using System.Text;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.Core.Services
{
    public class QueueProducer : IQueueProducer
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;

        private QueueProducer(IConnection connection, IChannel channel)
        {
            _connection = connection;
            _channel = channel;
        }

        // Factory method to create a QueueProducer with a connection and channel
        public static async Task<QueueProducer> CreateAsync()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();
            return new QueueProducer(connection, channel);
        }

        // Redirect to the correct HostedService depending on the message
        public void Send(MessageEnvelope brokerMessageDTO)
        {
            if (brokerMessageDTO.Command == "Clean Todos By User")
            {
                _channel.BasicPublishAsync(exchange: string.Empty,
                    routingKey: "todo_queue",
                    mandatory: true,
                    basicProperties: new BasicProperties { Persistent = true },
                    body: Encoding.UTF8.GetBytes(brokerMessageDTO.UserId.ToString())
                );
            }
        }

    }
}
`}
        </SyntaxHighlighter>
        <p>
          Consumer: <br />A Consumer listens to the queue, receives the
          messages, and processes them accordingly.
        </p>
        <SyntaxHighlighter
          language="csharp"
          style={okaidia}
          customStyle={{
            maxHeight: "300px",
            overflowY: "auto",
            fontSize: "14px",
            padding: "16px",
            borderRadius: "8px",
          }}
        >
          {`using Microsoft.Extensions.DependencyInjection;
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
                await todosService.CleanALlTodosByUser(userId);
            };

            await channel.BasicConsumeAsync("todo_queue", true, consumer);

            // HostedService runs while the application is running :)
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
`}
        </SyntaxHighlighter>
      </div>
      <div className="flex flex-col gap-5 p-5 rounded">
        <p className="text-lg font-bold">Implementation of Redis</p>
        <p>
          This service uses Redis to temporarily store a user’s todos in memory
          for fast access, reducing database load, and allows adding,
          retrieving, or deleting cached todos.
        </p>
        <SyntaxHighlighter
          language="csharp"
          style={okaidia}
          customStyle={{
            maxHeight: "300px",
            overflowY: "auto",
            fontSize: "14px",
            padding: "16px",
            borderRadius: "8px",
          }}
        >{`using StackExchange.Redis;
using System.Text.Json;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.Core.Services
{
    // cache service through redis
    public class TodoCacheService : ITodoCacheService
    {
        private readonly IDatabase _db;
        public TodoCacheService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        // Add all todos
        public async Task SetTodosAsync(List<TodoDTO> todos, Guid UserId)
        {
            var entries = todos.Select(todo =>
            new HashEntry(todo.Id.ToString(), JsonSerializer.Serialize(todo)))
                .ToArray();
            string HashKey = $"todos:{UserId}";

            await _db.HashSetAsync(HashKey, entries);
            await _db.KeyExpireAsync(HashKey, TimeSpan.FromMinutes(10));
        }

        // get all todos
        public async Task<List<TodoDTO>> GetAllTodosAsync(Guid UserId)
        {
            string HashKey = $"todos:{UserId}";
            var all = await _db.HashGetAllAsync(HashKey);
            return all.Select(x => JsonSerializer.Deserialize<TodoDTO>(x.Value)!)
                      .ToList();
        }

        // delete cache

        public async Task DeleteCache(Guid UserId)
        {
            string HashKey = $"todos:{UserId}";
            await _db.KeyDeleteAsync(HashKey);
        }
    }
}
`}</SyntaxHighlighter>
      </div>
    </div>
  );
}
