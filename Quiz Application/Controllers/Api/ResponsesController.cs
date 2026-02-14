using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quiz_Application.Models.Dtos;
using Quiz_Application.Services;

namespace Quiz_Application.Controllers.Api;

[ApiController]
[Route("api/responses")]
[Authorize]
public class ResponsesController(IResponseSvc svc) : ControllerBase
{
    /// <summary>POST /api/responses — save/update a user answer.</summary>
    [HttpPost]
    public async Task<IActionResult> Save([FromBody] SaveResponseReq req)
    {
        if (req.SessionId == Guid.Empty)
            return BadRequest(new { errors = new[] { "Session ID is required." } });

        var (success, responseId, errors) = await svc.SaveAsync(req.SessionId, req.QuestionId, req.AnswerText);

        if (!success)
            return BadRequest(new { errors });

        return Ok(new { success = true, responseId });
    }

    /// <summary>GET /api/responses/{sessionId} — all answers for a session.</summary>
    [HttpGet("{sessionId:guid}")]
    public async Task<IActionResult> GetBySession(Guid sessionId)
    {
        var map = await svc.GetBySessionAsync(sessionId);

        if (map.Count == 0)
            return NoContent();

        var dtos = map.Select(kv => new UserResponseDto(0, kv.Key, kv.Value));
        return Ok(dtos);
    }
}
