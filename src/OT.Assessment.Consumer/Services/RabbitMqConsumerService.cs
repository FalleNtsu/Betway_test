using OT.Assessment.App.Services;
using OT.Assessment.Consumer.Data.Models;
using OT.Assessment.Consumer.Repositories;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OT.Assessment.Consumer.Services
{
    internal class RabbitMqConsumerService : BackgroundService
    {
        private readonly IRabbitMqService _rabbitMq;
        private readonly IUserRepository _userRepository;
        private readonly IWagerRepository _wagerRepository;
        private readonly IUserStatsRepository _userStatsRepository;
        private readonly ILogger<RabbitMqConsumerService> _logger;

        public RabbitMqConsumerService(
        IRabbitMqService rabbitMq,
        ILogger<RabbitMqConsumerService> logger,
        IUserRepository userRepository,
        IWagerRepository wagerRepository,
        IUserStatsRepository userStatsRepository)
        {
            _rabbitMq = rabbitMq;
            _logger = logger;
            _userRepository = userRepository;
            _wagerRepository=wagerRepository;
            _userStatsRepository=userStatsRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RabbitMQ Consumer starting...");

            _rabbitMq.ConsumeAsync(async message =>
            {
                try
                {
                    var payload = JsonSerializer.Deserialize<WagerPayload>(message, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (payload != null)
                    {
                        await _userRepository.CreateUserIfNotExistsAsync(payload.AccountId, payload.Username);
                        await _wagerRepository.CreateWagerAsync(payload.WagerId, payload.GameName, payload.Provider, payload.Amount, payload.CreatedDateTime, payload.TransactionId, payload.AccountId);
                        await _userStatsRepository.CreateOrUpdateUserStatsAsync(payload.AccountId, payload.Amount);
                    }
                    else
                    {
                        _logger.LogWarning("Deserialized payload was null.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling message");
                }
            });

            _logger.LogInformation("RabbitMQ Consumer stopped.");
        }


    }
}
