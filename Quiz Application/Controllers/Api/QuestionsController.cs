using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quiz_Application.Services;

namespace Quiz_Application.Controllers.Api;

[ApiController]
[Route("api/questions")]
[Authorize]
public class QuestionsController(IQuestionSvc svc) : ControllerBase
{
    /// <summary>GET /api/questions — all questions for grid page.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await svc.GetAllAsync();
        return Ok(result);
    }

    /// <summary>GET /api/questions/detail — all questions with options for quiz page.</summary>
    [HttpGet("detail")]
    public async Task<IActionResult> GetAllDetail()
    {
        var result = await svc.GetAllDetailAsync();
        return Ok(result);
    }

    /// <summary>GET /api/questions/{orderNo} — single question by order.</summary>
    [HttpGet("{orderNo:int}")]
    public async Task<IActionResult> GetByOrder(int orderNo)
    {
        if (orderNo < 1)
            return BadRequest(new { error = "Order number must be >= 1." });

        var total = await svc.GetCountAsync();
        if (orderNo > total)
            return NotFound(new { error = $"Order number {orderNo} is out of range (1–{total})." });

        var dto = await svc.GetByOrderAsync(orderNo);
        return dto is not null ? Ok(dto) : NotFound();
    }
}
