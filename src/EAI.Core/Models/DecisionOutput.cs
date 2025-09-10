namespace EAI.Core.Models;

public class DecisionOutput
{
    public string Decision { get; set; } = string.Empty; // approve, reject, escalate
    public double ConfidenceScore { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public List<string> PolicyReferences { get; set; } = new();
    public string EscalationReason { get; set; } = string.Empty;
    public bool HumanReviewRequired { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string DecisionId { get; set; } = Guid.NewGuid().ToString();
    public TimeSpan ProcessingTime { get; set; }
}
