using EAI.Core.Interfaces;
using EAI.Core.Models;
using Microsoft.Extensions.Logging;

namespace EAI.Core.Services;

public class WorkflowOrchestrator : IWorkflowOrchestrator
{
    private readonly IPolicyEngine _policyEngine;
    private readonly IReasoningEngine _reasoningEngine;
    private readonly IAuditSystem _auditSystem;
    private readonly INotificationService _notificationService;
    private readonly ILogger<WorkflowOrchestrator> _logger;

    public WorkflowOrchestrator(
        IPolicyEngine policyEngine,
        IReasoningEngine reasoningEngine,
        IAuditSystem auditSystem,
        INotificationService notificationService,
        ILogger<WorkflowOrchestrator> logger)
    {
        _policyEngine = policyEngine;
        _reasoningEngine = reasoningEngine;
        _auditSystem = auditSystem;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<DecisionOutput> ProcessApprovalRequestAsync(ApprovalRequest request)
    {
        try
        {
            _logger.LogInformation("Processing approval request {RequestId}", request.Id);

            // Retrieve relevant policies
            var policies = await _policyEngine.RetrieveRelevantPoliciesAsync(request);

            // Resolve policy conflicts if any
            var conflictResolution = await _policyEngine.ResolvePolicyConflictsAsync(policies);

            // Make decision
            var decision = await _reasoningEngine.MakeDecisionAsync(request, policies);

            // Log audit trail
            await _auditSystem.LogDecisionAsync(decision, request, policies);

            // Handle escalation if needed
            if (_reasoningEngine.ShouldEscalate(decision, request))
            {
                await HandleEscalationAsync(request, decision);
            }

            // Send notification
            await _notificationService.SendDecisionNotificationAsync(request, decision);

            return decision;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing approval request {RequestId}", request.Id);
            throw;
        }
    }

    private async Task HandleEscalationAsync(ApprovalRequest request, DecisionOutput decision)
    {
        // Handle human escalation workflow
        await _notificationService.SendEscalationNotificationAsync(request, decision);
        _logger.LogWarning("Request {RequestId} escalated for human review", request.Id);
    }
}
