using EAI.Core.Interfaces;
using EAI.Core.Models;
using Microsoft.Extensions.Logging;

namespace EAI.Core.Services;

public class ReasoningEngine : IReasoningEngine
{
    private readonly ILLMService _llmService;
    private readonly IPolicyEngine _policyEngine;
    private readonly ILogger<ReasoningEngine> _logger;

    public ReasoningEngine(ILLMService llmService, IPolicyEngine policyEngine, ILogger<ReasoningEngine> logger)
    {
        _llmService = llmService;
        _policyEngine = policyEngine;
        _logger = logger;
    }

    public async Task<DecisionOutput> MakeDecisionAsync(ApprovalRequest request, IEnumerable<PolicyDocument> policies)
    {
        _logger.LogInformation("Making decision for request {RequestId}", request.Id);
        
        var startTime = DateTime.UtcNow;
        
        // Make autonomous approval decision
        var reasoning = await _llmService.GenerateReasoningAsync(request, policies);
        var decision = await _llmService.MakeDecisionAsync(request, policies, reasoning);
        
        var processingTime = DateTime.UtcNow - startTime;
        
        var result = new DecisionOutput
        {
            Decision = decision,
            ConfidenceScore = CalculateConfidence(decision, reasoning),
            Reasoning = reasoning,
            PolicyReferences = policies.Select(p => p.Id).ToList(),
            Timestamp = DateTime.UtcNow,
            ProcessingTime = processingTime
        };
        
        _logger.LogInformation("Decision made for request {RequestId}: {Decision} with confidence {Confidence}", 
            request.Id, result.Decision, result.ConfidenceScore);
        
        return result;
    }

    private double CalculateConfidence(string decision, string reasoning)
    {
        // Calculate confidence score for the decision
        return _llmService.CalculateConfidenceAsync(decision, reasoning).Result;
    }

    public bool ShouldEscalate(DecisionOutput decision, ApprovalRequest request)
    {
        // Determine if human escalation is needed
        var shouldEscalate = decision.ConfidenceScore < 0.7 || 
                           request.Amount > 10000 || 
                           request.Priority == "high";
        
        if (shouldEscalate)
        {
            _logger.LogWarning("Request {RequestId} should be escalated for human review", request.Id);
        }
        
        return shouldEscalate;
    }
}
