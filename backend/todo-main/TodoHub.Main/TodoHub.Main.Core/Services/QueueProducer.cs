using RabbitMQ.Client;
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
