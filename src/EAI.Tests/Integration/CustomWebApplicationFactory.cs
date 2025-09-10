using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EAI.Tests.Integration;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private static int _databaseCounter = 0;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<EAIDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add a unique in-memory database for each test
            var databaseName = $"EAI_Test_Database_{Interlocked.Increment(ref _databaseCounter)}_{Guid.NewGuid():N}";
            services.AddDbContext<EAIDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
                options.EnableSensitiveDataLogging();
            });

            // Build the service provider to ensure the database is created
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EAIDbContext>();
            context.Database.EnsureCreated();
        });

        builder.UseEnvironment("Development");
    }
}
