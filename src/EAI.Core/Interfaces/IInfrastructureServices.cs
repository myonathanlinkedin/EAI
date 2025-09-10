using EAI.Core.Models;

namespace EAI.Core.Interfaces;

public interface IPolicyRepository
{
    Task<IEnumerable<PolicyDocument>> GetAllAsync();
    Task<PolicyDocument?> GetByIdAsync(string id);
    Task<IEnumerable<PolicyDocument>> GetRelevantPoliciesAsync(string requestType, Dictionary<string, object> context);
    Task AddAsync(PolicyDocument policy);
    Task UpdateAsync(PolicyDocument policy);
    Task DeleteAsync(string id);
}

public interface IAuditRepository
{
    Task AddAsync(AuditLog auditLog);
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(DateTime startDate, DateTime endDate, AuditFilters filters);
    Task<IEnumerable<AuditLog>> GetPolicyUsageAsync(string policyId, DateTime startDate, DateTime endDate);
}

public interface ILLMService
{
    Task<string> GenerateReasoningAsync(ApprovalRequest request, IEnumerable<PolicyDocument> policies);
    Task<string> MakeDecisionAsync(ApprovalRequest request, IEnumerable<PolicyDocument> policies, string reasoning);
    Task<double> CalculateConfidenceAsync(string decision, string reasoning);
    Task<PolicyConflictResolution> ResolvePolicyConflictsAsync(IEnumerable<PolicyDocument> policies);
    Task<PolicyInterpretation> InterpretPolicyAsync(PolicyDocument policy, ApprovalRequest context);
}

public interface INotificationService
{
    Task SendDecisionNotificationAsync(ApprovalRequest request, DecisionOutput decision);
    Task SendEscalationNotificationAsync(ApprovalRequest request, DecisionOutput decision);
}

public interface IEncryptionService
{
    Task<string> EncryptAsync(string data);
    Task<string> DecryptAsync(string encryptedData);
}
