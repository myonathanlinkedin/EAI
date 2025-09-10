using EAI.Core.Interfaces;
using EAI.Core.Models;
using Microsoft.Extensions.Logging;

namespace EAI.Core.Services;

public class PolicyEngine : IPolicyEngine
{
    private readonly IPolicyRepository _policyRepository;
    private readonly ILLMService _llmService;
    private readonly ILogger<PolicyEngine> _logger;

    public PolicyEngine(IPolicyRepository policyRepository, ILLMService llmService, ILogger<PolicyEngine> logger)
    {
        _policyRepository = policyRepository;
        _llmService = llmService;
        _logger = logger;
    }

    public async Task<IEnumerable<PolicyDocument>> RetrieveRelevantPoliciesAsync(ApprovalRequest request)
    {
        _logger.LogInformation("Retrieving relevant policies for request {RequestId} of type {RequestType}", 
            request.Id, request.RequestType);
        
        // Retrieve policies relevant to the approval request
        return await _policyRepository.GetRelevantPoliciesAsync(request.RequestType, request.Context);
    }

    public async Task<PolicyConflictResolution> ResolvePolicyConflictsAsync(IEnumerable<PolicyDocument> policies)
    {
        _logger.LogInformation("Resolving policy conflicts for {PolicyCount} policies", policies.Count());
        
        // Resolve conflicts between multiple policies
        return await _llmService.ResolvePolicyConflictsAsync(policies);
    }

    public async Task<PolicyInterpretation> InterpretPolicyAsync(PolicyDocument policy, ApprovalRequest context)
    {
        _logger.LogInformation("Interpreting policy {PolicyId} for request {RequestId}", 
            policy.Id, context.Id);
        
        // Interpret policy in the context of the request
        return await _llmService.InterpretPolicyAsync(policy, context);
    }
}
