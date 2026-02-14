using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quiz_Application.Services;

namespace Quiz_Application.Controllers.Api;

[ApiController]
[Route("api/summary")]
[Authorize]
public class SummaryController(ISummarySvc svc) : ControllerBase
{
    /// <summary>GET /api/summary/{sessionId} — quiz results.</summary>
    [HttpGet("{sessionId:guid}")]
    public async Task<IActionResult> Get(Guid sessionId)
    {
        if (sessionId == Guid.Empty)
            return BadRequest(new { error = "Session ID is required." });

        var summary = await svc.BuildAsync(sessionId);
        return Ok(summary);
    }
}
