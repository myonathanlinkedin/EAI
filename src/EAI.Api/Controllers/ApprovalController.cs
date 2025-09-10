using EAI.Core.Interfaces;
using EAI.Core.Models;
using EAI.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApprovalController : ControllerBase
{
    private readonly IWorkflowOrchestrator _workflowOrchestrator;
    private readonly ILogger<ApprovalController> _logger;

    public ApprovalController(IWorkflowOrchestrator workflowOrchestrator, ILogger<ApprovalController> logger)
    {
        _workflowOrchestrator = workflowOrchestrator;
        _logger = logger;
    }

    [HttpPost("process")]
    public async Task<ActionResult<DecisionOutputDto>> ProcessApproval([FromBody] ApprovalRequestDto request)
    {
        try
        {
            var approvalRequest = new ApprovalRequest
            {
                RequestType = request.RequestType,
                RequesterId = request.RequesterId,
                Amount = request.Amount,
                Description = request.Description,
                SupportingDocuments = request.SupportingDocuments,
                Context = request.Context,
                Priority = request.Priority,
                Department = request.Department,
                Project = request.Project
            };

            var decision = await _workflowOrchestrator.ProcessApprovalRequestAsync(approvalRequest);

            return Ok(new DecisionOutputDto
            {
                Decision = decision.Decision,
                ConfidenceScore = decision.ConfidenceScore,
                Reasoning = decision.Reasoning,
                PolicyReferences = decision.PolicyReferences,
                EscalationReason = decision.EscalationReason,
                HumanReviewRequired = decision.HumanReviewRequired,
                Timestamp = decision.Timestamp,
                ProcessingTime = decision.ProcessingTime.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing approval request");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("audit/{requestId}")]
    public async Task<ActionResult<AuditLog>> GetAuditTrail(string requestId)
    {
        // Get audit trail for specific request
        return Ok();
    }

    [HttpGet("status/{requestId}")]
    public async Task<ActionResult<ApprovalStatus>> GetRequestStatus(string requestId)
    {
        // Get current status of approval request
        return Ok();
    }
}
