using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace OpenTelemetryDemo.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly ActivitySource _activitySource;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
        _activitySource = new ActivitySource("OpenTelemetryDemo.WeatherForecast");
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        using var activity = _activitySource.StartActivity("GetWeatherForecast");
        
        _logger.LogInformation("Weather forecast requested");

        var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                Summaries[Random.Shared.Next(Summaries.Length)]
            ))
            .ToArray();

        activity?.SetTag("forecast.count", forecast.Length);

        return forecast;
    }
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}