using EAI.Core.Models;

namespace EAI.Core.Interfaces;

public interface IPolicyEngine
{
    Task<IEnumerable<PolicyDocument>> RetrieveRelevantPoliciesAsync(ApprovalRequest request);
    Task<PolicyConflictResolution> ResolvePolicyConflictsAsync(IEnumerable<PolicyDocument> policies);
    Task<PolicyInterpretation> InterpretPolicyAsync(PolicyDocument policy, ApprovalRequest context);
}

public interface IReasoningEngine
{
    Task<DecisionOutput> MakeDecisionAsync(ApprovalRequest request, IEnumerable<PolicyDocument> policies);
    bool ShouldEscalate(DecisionOutput decision, ApprovalRequest request);
}

public interface IAuditSystem
{
    Task LogDecisionAsync(DecisionOutput decision, ApprovalRequest request, IEnumerable<PolicyDocument> policies);
    Task<AuditReport> GenerateAuditReportAsync(DateTime startDate, DateTime endDate, AuditFilters filters);
    Task<IEnumerable<AuditLog>> TracePolicyUsageAsync(string policyId, DateTime startDate, DateTime endDate);
}

public interface IWorkflowOrchestrator
{
    Task<DecisionOutput> ProcessApprovalRequestAsync(ApprovalRequest request);
}
