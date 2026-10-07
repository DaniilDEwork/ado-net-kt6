using Kt6UpdateApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kt6UpdateApi.Controllers;

[ApiController]
[Route("api/logs")]
public class LogsController(UpdateLogStore logStore, ILogger<LogsController> logger) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {
            return Ok(logStore.GetRecent());
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            logger.LogError(exception, "Не удалось прочитать журнал");
            return StatusCode(500, new { message = "Не удалось прочитать журнал запросов" });
        }
    }
}
