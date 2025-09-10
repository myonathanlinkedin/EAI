using Microsoft.AspNetCore.Mvc;

namespace EAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }

    [HttpGet("ready")]
    public IActionResult Ready()
    {
        // Add readiness checks here (database, external services, etc.)
        return Ok(new { Status = "Ready", Timestamp = DateTime.UtcNow });
    }
}
