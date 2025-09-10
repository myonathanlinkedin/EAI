using EAI.Core.Interfaces;
using EAI.Core.Models;
using Microsoft.Extensions.Logging;

namespace EAI.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public async Task SendDecisionNotificationAsync(ApprovalRequest request, DecisionOutput decision)
    {
        _logger.LogInformation("Sending decision notification for request {RequestId} with decision {Decision}", 
            request.Id, decision.Decision);
        
        // In a real implementation, this would send emails, push notifications, etc.
        await Task.Delay(100); // Simulate async operation
        
        _logger.LogInformation("Decision notification sent for request {RequestId}", request.Id);
    }

    public async Task SendEscalationNotificationAsync(ApprovalRequest request, DecisionOutput decision)
    {
        _logger.LogWarning("Sending escalation notification for request {RequestId}", request.Id);
        
        // In a real implementation, this would notify managers, create tickets, etc.
        await Task.Delay(100); // Simulate async operation
        
        _logger.LogWarning("Escalation notification sent for request {RequestId}", request.Id);
    }
}
