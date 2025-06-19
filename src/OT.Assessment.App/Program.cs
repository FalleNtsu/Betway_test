using Microsoft.Extensions.Options;
using OT.Assessment.App.Data;
using OT.Assessment.App.Repositories;
using OT.Assessment.App.Services;
using OT.Assessment.App.Static;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckl
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Configuration
    .AddUserSecrets<Program>()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));

builder.Services.AddSingleton<IRabbitMqService>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
    var rabbit = RabbitMqService.CreateInstanceAsync(settings).GetAwaiter().GetResult();
    return rabbit;
});

builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWagerRepository, WagerRepository>();
builder.Services.AddScoped<IUserStatsRepository, UserStatsRepository>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opts =>
    {
        opts.EnableTryItOutByDefault();
        opts.DocumentTitle = "OT Assessment App";
        opts.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
