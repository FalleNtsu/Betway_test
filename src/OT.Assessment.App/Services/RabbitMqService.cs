
using OT.Assessment.App.Models.Requests;
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
        private IConnectionFactory _connectionFactory;
        private readonly ILogger<RabbitMqService> _logger;
        private const string QueueName = "casino_wager_events";
        private const string dlQueueName = "casino_wager_events_dlq";
        private readonly SemaphoreSlim _connectionLock = new(1, 1);

        public RabbitMqService(RabbitMqSettings settings, ILogger<RabbitMqService> logger)
        {
            _settings = settings;
            _logger=logger;
        }

        public static async Task<RabbitMqService> CreateInstanceAsync(RabbitMqSettings settings, ILogger<RabbitMqService> logger)
        {
            var instance = new RabbitMqService(settings, logger);
            await instance.InitializeAsync();
            return instance;
        }

        private async Task InitializeAsync()
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = _settings.Host,
                UserName = _settings.Username,
                Password = _settings.Password
            };
            _connection = await _connectionFactory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await DeclareQueueIfNotExistsAsync(dlQueueName);

            var mainQueueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "" }, 
                { "x-dead-letter-routing-key", dlQueueName }
            };

            await DeclareQueueIfNotExistsAsync(QueueName, mainQueueArgs);
        }

        public async Task PublishAsync<T>(T message)
        {
            if (_channel is null)
                throw new InvalidOperationException("Channel is not initialized.");

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            const int maxRetries = 3;
            int attempt = 0;
            while (true)
            {
                try
                {
                    await CheckConnectionAndChannel();
                    await _channel.BasicPublishAsync(
                            exchange: string.Empty,
                            routingKey: QueueName,
                            body: body);
                    break;
                }
                catch (Exception ex)
                {
                    attempt++;

                    _logger.LogWarning(ex, "Failed to publish message to RabbitMQ. Attempt {Attempt} of {MaxRetries}", attempt, maxRetries);

                    if (attempt >= maxRetries)
                    {
                        _logger.LogError(ex, "Exceeded max retry attempts. Message not published.");
                        throw;
                    }

                    await Task.Delay(500 * attempt);
                }
            }
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

        private async Task CheckConnectionAndChannel()
        {
            if (_connection != null && _connection.IsOpen &&
                _channel != null && _channel.IsOpen)
            {
                return;
            }

            await _connectionLock.WaitAsync();
            try
            {
                if (_connection == null || !_connection.IsOpen)
                {
                    _connection = await _connectionFactory.CreateConnectionAsync();
                    _logger.LogInformation("Reconnected to RabbitMQ.");
                }

                if (_channel == null || !_channel.IsOpen)
                {
                    _channel = await _connection.CreateChannelAsync();
                    await DeclareQueueIfNotExistsAsync(dlQueueName);
                    await DeclareQueueIfNotExistsAsync(QueueName, new Dictionary<string, object>
                        {
                            { "x-dead-letter-exchange", "" },
                            { "x-dead-letter-routing-key", dlQueueName }
                        });

                    _logger.LogInformation("Recreated RabbitMQ channel and queues.");
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public async Task<IList<CasinoWager>> GetDeadLetterMessagesAsync(int maxCount = 50)
        {
            var wagers = new List<CasinoWager>();

            for (int i = 0; i < maxCount; i++)
            {
                var result = await _channel.BasicGetAsync("casino_wager_events_dlq", autoAck: false);
                if (result == null) break;

                var json = Encoding.UTF8.GetString(result.Body.ToArray());
                var wager = JsonSerializer.Deserialize<CasinoWager>(json);
                wagers.Add(wager);

                // Requeue the message so we don't lose it when just viewing
                await _channel.BasicNackAsync(result.DeliveryTag, false, requeue: true);
            }

            return wagers;
        }

        private async Task DeclareQueueIfNotExistsAsync(string queueName, Dictionary<string, object>? args = null)
        {
            try
            {
                await _channel.QueueDeclarePassiveAsync(queueName);
                _logger.LogInformation("Queue '{Queue}' already exists. Skipping declaration.", queueName);
            }
            catch (RabbitMQ.Client.Exceptions.OperationInterruptedException ex)
            {
                _logger.LogWarning(ex, "Queue '{Queue}' does not exist or mismatched. Declaring...", queueName);

                // After this error, RabbitMQ may close the channel
                if (!_channel.IsOpen)
                {
                    _logger.LogWarning("Channel was closed by broker. Re-creating channel...");
                    _channel = await _connection.CreateChannelAsync();
                }

                await _channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: args);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "QueueDeclareAsync for '{Queue}' timed out (TaskCanceledException).");
                throw;
            }
        }

    }
}
