namespace EAI.Core.Models;

public class AuditLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string RequestId { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public string PolicyReferences { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
    public string EncryptedData { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}

public class AuditReport
{
    public string Period { get; set; } = string.Empty;
    public int TotalDecisions { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int EscalatedCount { get; set; }
    public double AverageConfidence { get; set; }
    public IEnumerable<AuditLog> Logs { get; set; } = new List<AuditLog>();
}

public class AuditFilters
{
    public string? Department { get; set; }
    public string? RequestType { get; set; }
    public double? MinConfidenceScore { get; set; }
    public double? MaxConfidenceScore { get; set; }
    public string? Decision { get; set; }
}

public class PolicyConflictResolution
{
    public IEnumerable<PolicyDocument> ResolvedPolicies { get; set; } = new List<PolicyDocument>();
    public string ConflictReason { get; set; } = string.Empty;
    public string ResolutionStrategy { get; set; } = string.Empty;
}

public class PolicyInterpretation
{
    public string Interpretation { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> ApplicableRules { get; set; } = new();
    public string Reasoning { get; set; } = string.Empty;
}
