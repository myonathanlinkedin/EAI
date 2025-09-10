namespace EAI.Api.DTOs;

public class ApprovalRequestDto
{
    public string RequestType { get; set; } = string.Empty;
    public string RequesterId { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> SupportingDocuments { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();
    public string Priority { get; set; } = "normal";
    public string Department { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
}

public class DecisionOutputDto
{
    public string Decision { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public List<string> PolicyReferences { get; set; } = new();
    public string EscalationReason { get; set; } = string.Empty;
    public bool HumanReviewRequired { get; set; }
    public DateTime Timestamp { get; set; }
    public string ProcessingTime { get; set; } = string.Empty;
}

public class PolicyDocumentDto
{
    public string PolicyName { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Department { get; set; } = string.Empty;
}
