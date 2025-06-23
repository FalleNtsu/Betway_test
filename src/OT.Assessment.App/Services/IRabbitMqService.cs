using OT.Assessment.App.Models.Requests;

namespace OT.Assessment.App.Services
{
    public interface IRabbitMqService
    {
        Task PublishAsync<T>(T message);
        public void ConsumeAsync(Func<string, Task> handleMessage);
        Task<IList<CasinoWager>> GetDeadLetterMessagesAsync(int maxCount = 50);
    }
}
