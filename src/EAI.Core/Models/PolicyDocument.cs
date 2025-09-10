namespace EAI.Core.Models;

public class PolicyDocument
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PolicyName { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public List<PolicyRule> Rules { get; set; } = new();
    public string Department { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class PolicyRule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Condition { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public double ConfidenceThreshold { get; set; }
    public string Priority { get; set; } = "normal";
}
