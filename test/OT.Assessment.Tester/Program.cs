// See https://aka.ms/new-console-template for more information


using System.Net.Http.Json;

var bg = new BogusGenerator();
var total = bg.Generate();

using var httpClient = new HttpClient();
var loginRequest = new
{
    APIKey = Guid.Parse("586214D2-4CBB-4914-99FC-7B27EDFBC8FA"),
    Password = "OT_Assessment_Password"
};

var authResponse = await httpClient.PostAsJsonAsync("https://localhost:7120/api/auth/login", loginRequest);

authResponse.EnsureSuccessStatusCode();

var json = await authResponse.Content.ReadAsStringAsync();
using var jsonDoc = JsonDocument.Parse(json);
var token = jsonDoc.RootElement.GetProperty("token").GetString();

var scenario = Scenario.Create("hello_world_scenario", async context =>
    {
        var body = JsonSerializer.Serialize(total[(int)context.InvocationNumber]);
        

        var request =
            Http.CreateRequest("POST", "https://localhost:7120/api/Player/CasinoWager")
                .WithHeader("Accept", "application/json")
                .WithHeader("Authorization", $"Bearer {token}")
                .WithBody(new StringContent($"{body}", Encoding.UTF8, "application/json"));

        var response = await Http.Send(httpClient, request);

        if (response.StatusCode == "OK") return Response.Ok();
        return Response.Fail(body, response.StatusCode, response.Message, response.SizeBytes);
    })
    .WithoutWarmUp()
    .WithLoadSimulations(
        Simulation.IterationsForInject(rate: 500,
            interval: TimeSpan.FromSeconds(2),
            iterations: 7000)
    );

NBomberRunner
    .RegisterScenarios(scenario)
    .WithWorkerPlugins(new HttpMetricsPlugin(new[] { HttpVersion.Version1 }))
    .WithoutReports()
    .Run();
Console.WriteLine("Press Enter to exit...");
Console.ReadLine();