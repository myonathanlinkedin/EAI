using EAI.Core.Interfaces;
using EAI.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EAI.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEAICore(this IServiceCollection services)
    {
        // Register core services
        services.AddScoped<IPolicyEngine, PolicyEngine>();
        services.AddScoped<IReasoningEngine, ReasoningEngine>();
        services.AddScoped<IAuditSystem, AuditSystem>();
        services.AddScoped<IWorkflowOrchestrator, WorkflowOrchestrator>();

        return services;
    }
}
