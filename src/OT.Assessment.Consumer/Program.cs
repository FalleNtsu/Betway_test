using Microsoft.Extensions.Configuration;
using OT.Assessment.App.Services;
using OT.Assessment.App.Static;
using OT.Assessment.Consumer.Data;
using OT.Assessment.Consumer.Repositories;
using OT.Assessment.Consumer.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<Program>()
            .Build();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        var rabbitMqSettings = configuration.GetSection("RabbitMq").Get<RabbitMqSettings>();
        services.AddSingleton(rabbitMqSettings);

        services.AddSingleton<IRabbitMqService>(sp =>
            RabbitMqService.CreateInstanceAsync(rabbitMqSettings).GetAwaiter().GetResult());
        //Repositories
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWagerRepository, WagerRepository>();
        services.AddScoped<IUserStatsRepository, UserStatsRepository>();
        

        //Services
        services.AddHostedService<RabbitMqConsumerService>();
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application started {time:yyyy-MM-dd HH:mm:ss}", DateTime.Now);

await host.RunAsync();

logger.LogInformation("Application ended {time:yyyy-MM-dd HH:mm:ss}", DateTime.Now);