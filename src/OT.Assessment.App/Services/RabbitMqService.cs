
using OT.Assessment.App.Static;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace OT.Assessment.App.Services
{
    
public class RabbitMqService : IRabbitMqService, IAsyncDisposable
    {
        private readonly RabbitMqSettings _settings;
        private IConnection _connection;
        private IChannel _channel;
        private const string QueueName = "casino_wager_events";

        public RabbitMqService(RabbitMqSettings settings)
        {
            _settings = settings;
        }

        public static async Task<RabbitMqService> CreateInstanceAsync(RabbitMqSettings settings)
        {
            var instance = new RabbitMqService(settings);
            await instance.InitializeAsync();
            return instance;
        }

        private async Task InitializeAsync()
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                UserName = _settings.Username,
                Password = _settings.Password
            };
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(
                queue: QueueName,
                durable: false,
                exclusive: false,
                autoDelete: false);
        }

        public async Task PublishAsync<T>(T message)
        {
            if (_channel is null)
                throw new InvalidOperationException("Channel is not initialized.");

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: QueueName,
                body: body);
        }

        public void ConsumeAsync(Func<string, Task> handleMessage)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    await handleMessage(message);
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception)
                {
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
            };

            _channel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel is not null)
                await _channel.CloseAsync();

            if (_connection is not null)
                await _connection.CloseAsync();
        }
    }
}
