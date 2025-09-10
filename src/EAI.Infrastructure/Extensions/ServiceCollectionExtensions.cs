using EAI.Core.Extensions;
using EAI.Core.Interfaces;
using EAI.Infrastructure.Data;
using EAI.Infrastructure.Repositories;
using EAI.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EAI.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEAIInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Entity Framework with in-memory database by default
        var useInMemory = configuration.GetValue<bool>("Database:UseInMemory", true);
        
        if (useInMemory)
        {
            services.AddDbContext<EAIDbContext>(options =>
                options.UseInMemoryDatabase("EAI_Database"));
        }
        else
        {
            services.AddDbContext<EAIDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        }

        // Add repositories
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();

        // Add services with extended timeout for LLM processing
        services.AddHttpClient<ILLMService, LLMService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(120); // 2 minutes for LLM processing
        });
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IEncryptionService, EncryptionService>();

        return services;
    }
}
