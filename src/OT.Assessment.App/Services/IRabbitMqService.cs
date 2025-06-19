namespace OT.Assessment.App.Services
{
    public interface IRabbitMqService
    {
        Task PublishAsync<T>(T message);
        public void ConsumeAsync(Func<string, Task> handleMessage);
    }
}
