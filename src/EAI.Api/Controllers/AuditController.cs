using EAI.Core.Interfaces;
using EAI.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace EAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditSystem _auditSystem;
    private readonly ILogger<AuditController> _logger;

    public AuditController(IAuditSystem auditSystem, ILogger<AuditController> logger)
    {
        _auditSystem = auditSystem;
        _logger = logger;
    }

    [HttpGet("report")]
    public async Task<ActionResult<AuditReport>> GenerateAuditReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] AuditFilters filters)
    {
        var report = await _auditSystem.GenerateAuditReportAsync(startDate, endDate, filters);
        return Ok(report);
    }

    [HttpGet("policy-usage/{policyId}")]
    public async Task<ActionResult<IEnumerable<AuditLog>>> TracePolicyUsage(
        string policyId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var logs = await _auditSystem.TracePolicyUsageAsync(policyId, startDate, endDate);
        return Ok(logs);
    }
}
