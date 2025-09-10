using EAI.Core.Interfaces;
using EAI.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EAI.Infrastructure.Services;

public class LLMService : ILLMService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LLMService> _logger;
    private readonly string _lmStudioUrl;
    private readonly string _modelName;
    private readonly string _provider;

    public LLMService(HttpClient httpClient, IConfiguration configuration, ILogger<LLMService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _provider = _configuration["LLM:Provider"] ?? "LMStudio";
        _lmStudioUrl = _configuration["LLM:LMStudioUrl"] ?? "http://localhost:1234";
        _modelName = _configuration["LLM:Model"] ?? "qwen2.5-7b-instruct-1m";
    }

    public async Task<string> GenerateReasoningAsync(ApprovalRequest request, IEnumerable<PolicyDocument> policies)
    {
        _logger.LogInformation("Generating reasoning for request {RequestId}", request.Id);
        
        var prompt = BuildReasoningPrompt(request, policies);
        var response = await CallLMStudioAsync(prompt);
        
        return response ?? $"Based on the {request.RequestType} request for ${request.Amount} in the {request.Department} department, " +
                          $"considering {policies.Count()} applicable policies, " +
                          $"the request appears to be {(request.Amount <= 1000 ? "within normal limits" : "requiring additional review")}.";
    }

    public async Task<string> MakeDecisionAsync(ApprovalRequest request, IEnumerable<PolicyDocument> policies, string reasoning)
    {
        _logger.LogInformation("Making decision for request {RequestId}", request.Id);
        
        var prompt = BuildDecisionPrompt(request, policies, reasoning);
        var response = await CallLMStudioAsync(prompt);
        
        // Parse the response to extract decision
        var decision = ParseDecision(response);
        
        return decision ?? DetermineFallbackDecision(request);
    }

    public async Task<double> CalculateConfidenceAsync(string decision, string reasoning)
    {
        _logger.LogInformation("Calculating confidence for decision: {Decision}", decision);
        
        var prompt = BuildConfidencePrompt(decision, reasoning);
        var response = await CallLMStudioAsync(prompt);
        
        if (double.TryParse(response, out var confidence))
        {
            return Math.Max(0.0, Math.Min(1.0, confidence));
        }
        
        // Fallback confidence calculation
        return decision switch
        {
            "approve" when reasoning.Contains("normal limits") => 0.9,
            "approve" => 0.8,
            "escalate" => 0.7,
            "reject" => 0.85,
            _ => 0.5
        };
    }

    public async Task<PolicyConflictResolution> ResolvePolicyConflictsAsync(IEnumerable<PolicyDocument> policies)
    {
        _logger.LogInformation("Resolving policy conflicts for {PolicyCount} policies", policies.Count());
        
        var prompt = BuildConflictResolutionPrompt(policies);
        var response = await CallLMStudioAsync(prompt);
        
        // Parse response and create resolution
        var resolvedPolicies = policies.OrderByDescending(p => p.Version).ToList();
        
        return new PolicyConflictResolution
        {
            ResolvedPolicies = resolvedPolicies,
            ConflictReason = response ?? "Version-based resolution applied",
            ResolutionStrategy = "PriorityBased"
        };
    }

    public async Task<PolicyInterpretation> InterpretPolicyAsync(PolicyDocument policy, ApprovalRequest context)
    {
        _logger.LogInformation("Interpreting policy {PolicyId} for request {RequestId}", policy.Id, context.Id);
        
        var prompt = BuildPolicyInterpretationPrompt(policy, context);
        var response = await CallLMStudioAsync(prompt);
        
        return new PolicyInterpretation
        {
            Interpretation = response ?? $"Policy {policy.PolicyName} applies to {context.RequestType} requests",
            Confidence = 0.8,
            ApplicableRules = policy.Rules.Select(r => r.Id).ToList(),
            Reasoning = response ?? $"Policy is active and covers the request type {context.RequestType}"
        };
    }

    private async Task<string?> CallLMStudioAsync(string prompt)
    {
        try
        {
            var requestBody = new
            {
                model = _modelName,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.3,
                max_tokens = 50,
                stream = false
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_lmStudioUrl}/v1/chat/completions", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var lmStudioResponse = JsonSerializer.Deserialize<LMStudioResponse>(responseContent);
                var rawContent = lmStudioResponse?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
                
                if (string.IsNullOrEmpty(rawContent))
                {
                    _logger.LogWarning("Empty response from LM Studio");
                    return null;
                }
                
                // Process the response to extract the actual decision
                var processedContent = ProcessLLMResponse(rawContent);
                _logger.LogInformation("LLM Response: {RawResponse}", rawContent);
                _logger.LogInformation("Processed Response: {ProcessedResponse}", processedContent);
                
                return processedContent;
            }
            else
            {
                _logger.LogWarning("LM Studio API call failed with status: {StatusCode}", response.StatusCode);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling LM Studio");
            return null;
        }
    }

    private string BuildReasoningPrompt(ApprovalRequest request, IEnumerable<PolicyDocument> policies)
    {
        var policiesText = string.Join("\n", policies.Select(p => $"- {p.PolicyName}: {p.Content}"));
        
        return $@"You are an AI assistant analyzing an approval request. Please provide detailed reasoning for your decision.

REQUEST DETAILS:
- Type: {request.RequestType}
- Amount: ${request.Amount}
- Department: {request.Department}
- Description: {request.Description}
- Priority: {request.Priority}

APPLICABLE POLICIES:
{policiesText}

Please analyze this request against the policies and provide clear reasoning for your decision. Consider:
1. Policy compliance
2. Risk assessment
3. Business impact
4. Any exceptions or special circumstances

Provide your reasoning in 2-3 sentences:";
    }

    private string BuildDecisionPrompt(ApprovalRequest request, IEnumerable<PolicyDocument> policies, string reasoning)
    {
        return $@"Make a decision for this approval request.

REQUEST: {request.RequestType} for ${request.Amount} in {request.Department}
ANALYSIS: {reasoning}

IMPORTANT: Respond with ONLY the decision word. Do not include any reasoning or explanation.

Choose one: approve, reject, or escalate

Decision:";
    }

    private string BuildConfidencePrompt(string decision, string reasoning)
    {
        return $@"Rate your confidence in this decision on a scale of 0.0 to 1.0.

Decision: {decision}
Reasoning: {reasoning}

Consider:
- Clarity of policies
- Uniqueness of request
- Potential risks
- Business impact

Confidence score (0.0-1.0):";
    }

    private string BuildConflictResolutionPrompt(IEnumerable<PolicyDocument> policies)
    {
        var policiesText = string.Join("\n", policies.Select(p => $"- {p.PolicyName} (v{p.Version}): {p.Content}"));
        
        return $@"Analyze these policies for conflicts and provide resolution guidance:

POLICIES:
{policiesText}

Identify any conflicts and suggest resolution strategy. Focus on:
1. Conflicting rules or thresholds
2. Priority resolution methods
3. Business impact considerations

Resolution guidance:";
    }

    private string BuildPolicyInterpretationPrompt(PolicyDocument policy, ApprovalRequest context)
    {
        return $@"Interpret this policy in the context of the given request.

POLICY: {policy.PolicyName}
Content: {policy.Content}
Rules: {string.Join(", ", policy.Rules.Select(r => r.Condition))}

REQUEST CONTEXT:
- Type: {context.RequestType}
- Amount: ${context.Amount}
- Department: {context.Department}

Provide interpretation focusing on:
1. Applicability to the request
2. Relevant rules and conditions
3. Expected outcome

Interpretation:";
    }

    private string? ParseDecision(string? response)
    {
        if (string.IsNullOrEmpty(response))
            return null;

        var lowerResponse = response.ToLower().Trim();
        
        if (lowerResponse.Contains("approve"))
            return "approve";
        if (lowerResponse.Contains("reject"))
            return "reject";
        if (lowerResponse.Contains("escalate"))
            return "escalate";
            
        return null;
    }

    private string DetermineFallbackDecision(ApprovalRequest request)
    {
        // Simple fallback logic based on amount and policies
        if (request.Amount <= 100)
            return "approve";
        else if (request.Amount <= 1000)
            return "approve";
        else if (request.Amount <= 10000)
            return "escalate";
        else
            return "escalate";
            }
        
        private string ProcessLLMResponse(string rawResponse)
        {
            if (string.IsNullOrEmpty(rawResponse))
                return "No response generated";
            
            // Remove <think> tags and extract the actual decision
            var response = rawResponse;
            
            // If response contains <think> tags, extract content after </think>
            if (response.Contains("<think>") && response.Contains("</think>"))
            {
                var thinkEndIndex = response.IndexOf("</think>");
                if (thinkEndIndex != -1)
                {
                    response = response.Substring(thinkEndIndex + 8).Trim();
                }
            }
            
            // If response still contains <think> tag (incomplete), try to extract what's after it
            if (response.Contains("<think>"))
            {
                var thinkStartIndex = response.IndexOf("<think>");
                if (thinkStartIndex != -1)
                {
                    response = response.Substring(thinkStartIndex + 7).Trim();
                }
            }
            
            // Clean up the response
            response = response.Trim();
            
            // If response is empty or too short, return a fallback
            if (string.IsNullOrEmpty(response) || response.Length < 3)
            {
                return "Unable to generate decision";
            }
            
            return response;
        }
    }

    public class LMStudioResponse
{
    public LMStudioChoice[]? Choices { get; set; }
}

public class LMStudioChoice
{
    public LMStudioMessage? Message { get; set; }
}

public class LMStudioMessage
{
    public string? Content { get; set; }
}
