using EAI.Core.Extensions;
using EAI.Core.Interfaces;
using EAI.Core.Models;
using EAI.Infrastructure.Extensions;
using EAI.Api.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/eai-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add EAI services
builder.Services.AddEAICore();
builder.Services.AddEAIInfrastructure(builder.Configuration);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Add custom middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
// app.UseMiddleware<AuthenticationMiddleware>(); // Temporarily disabled for testing
app.UseMiddleware<AuditMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Seed database if using in-memory
var seedData = builder.Configuration.GetValue<bool>("Database:SeedData", true);
if (seedData)
{
    using var scope = app.Services.CreateScope();
    await EAI.Infrastructure.Data.DatabaseSeeder.SeedDatabaseAsync(scope.ServiceProvider);
}

try
{
    Log.Information("Starting EAI API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for testing
public partial class Program { }
