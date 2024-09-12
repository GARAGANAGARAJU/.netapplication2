using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Custom metrics for the application
var appMeter = new Meter("OpenTelemetryDemo.Metrics", "1.0.0");
var requestCounter = appMeter.CreateCounter<int>("app.request.count", description: "Counts the number of requests");

var otel = builder.Services.AddOpenTelemetry();

// Configure OpenTelemetry Resources with the application name
otel.ConfigureResource(resource => resource
    .AddService(serviceName: builder.Environment.ApplicationName));

// Add Metrics
otel.WithMetrics(metrics => metrics
    .AddAspNetCoreInstrumentation()
    .AddMeter(appMeter.Name)
    .AddMeter("Microsoft.AspNetCore.Hosting")
    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
    .AddPrometheusExporter());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Configure the Prometheus scraping endpoint
app.MapPrometheusScrapingEndpoint();

// Add a simple endpoint for testing
app.MapGet("/hello", (ILogger<Program> logger) =>
{
    requestCounter.Add(1);
    logger.LogInformation("Hello endpoint called");

    return "Hello, World!";
});

app.Run();
