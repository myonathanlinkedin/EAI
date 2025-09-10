namespace EAI.Core.Models;

public class ApprovalRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string RequestType { get; set; } = string.Empty; // expense, leave, purchase, etc.
    public string RequesterId { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> SupportingDocuments { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Priority { get; set; } = "normal";
    public string Department { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected,
    Escalated,
    UnderReview
}
