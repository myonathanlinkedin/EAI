using EAI.Core.Interfaces;
using EAI.Core.Models;
using EAI.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PolicyController : ControllerBase
{
    private readonly IPolicyRepository _policyRepository;
    private readonly ILogger<PolicyController> _logger;

    public PolicyController(IPolicyRepository policyRepository, ILogger<PolicyController> logger)
    {
        _policyRepository = policyRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PolicyDocument>>> ListPolicies()
    {
        var policies = await _policyRepository.GetAllAsync();
        return Ok(policies);
    }

    [HttpPost]
    public async Task<ActionResult<PolicyDocument>> AddPolicy([FromBody] PolicyDocumentDto policyDto)
    {
        var policy = new PolicyDocument
        {
            PolicyName = policyDto.PolicyName,
            PolicyType = policyDto.PolicyType,
            Content = policyDto.Content,
            Version = policyDto.Version,
            EffectiveDate = policyDto.EffectiveDate,
            ExpiryDate = policyDto.ExpiryDate,
            Department = policyDto.Department
        };

        await _policyRepository.AddAsync(policy);
        return CreatedAtAction(nameof(GetPolicy), new { id = policy.Id }, policy);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PolicyDocument>> GetPolicy(string id)
    {
        var policy = await _policyRepository.GetByIdAsync(id);
        if (policy == null)
            return NotFound();

        return Ok(policy);
    }
}
