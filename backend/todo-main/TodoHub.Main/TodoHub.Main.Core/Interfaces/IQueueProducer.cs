using TodoHub.Main.Core.DTOs.Request;


namespace TodoHub.Main.Core.Interfaces
{
    public interface IQueueProducer
    {
        void Send(MessageEnvelope brokerMessageDTO);
    }
}