using EAI.Core.Interfaces;
using EAI.Core.Models;
using Microsoft.Extensions.Logging;

namespace EAI.Core.Services;

public class AuditSystem : IAuditSystem
{
    private readonly IAuditRepository _auditRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<AuditSystem> _logger;

    public AuditSystem(IAuditRepository auditRepository, IEncryptionService encryptionService, ILogger<AuditSystem> logger)
    {
        _auditRepository = auditRepository;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task LogDecisionAsync(DecisionOutput decision, ApprovalRequest request, IEnumerable<PolicyDocument> policies)
    {
        _logger.LogInformation("Logging decision for request {RequestId}", request.Id);
        
        // Log decision with full audit trail
        var auditLog = new AuditLog
        {
            RequestId = request.Id,
            Decision = decision.Decision,
            ConfidenceScore = decision.ConfidenceScore,
            Reasoning = decision.Reasoning,
            PolicyReferences = string.Join(",", decision.PolicyReferences),
            Timestamp = DateTime.UtcNow,
            UserId = request.RequesterId,
            EncryptedData = await _encryptionService.EncryptAsync(decision.Reasoning)
        };

        await _auditRepository.AddAsync(auditLog);
        
        _logger.LogInformation("Decision logged for request {RequestId}", request.Id);
    }

    public async Task<AuditReport> GenerateAuditReportAsync(DateTime startDate, DateTime endDate, AuditFilters filters)
    {
        _logger.LogInformation("Generating audit report from {StartDate} to {EndDate}", startDate, endDate);
        
        // Generate compliance audit report
        var logs = await _auditRepository.GetAuditLogsAsync(startDate, endDate, filters);
        return new AuditReport
        {
            Period = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
            TotalDecisions = logs.Count(),
            ApprovedCount = logs.Count(l => l.Decision == "approve"),
            RejectedCount = logs.Count(l => l.Decision == "reject"),
            EscalatedCount = logs.Count(l => l.Decision == "escalate"),
            AverageConfidence = logs.Any() ? logs.Average(l => l.ConfidenceScore) : 0,
            Logs = logs
        };
    }

    public async Task<IEnumerable<AuditLog>> TracePolicyUsageAsync(string policyId, DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Tracing policy usage for policy {PolicyId} from {StartDate} to {EndDate}", 
            policyId, startDate, endDate);
        
        // Trace how policies were used in decisions
        return await _auditRepository.GetPolicyUsageAsync(policyId, startDate, endDate);
    }
}
