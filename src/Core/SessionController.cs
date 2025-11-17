using Microsoft.AspNetCore.Mvc;

namespace tero_session.src.Core;

[ApiController]
[Route("session")]
public class SessionController(ILogger<SessionController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> SessionHealth()
    {
        try
        {
            await Task.Run(() =>
                   {
                       Console.WriteLine("Hello from task");
                   });

            return Ok("Session");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error");
            return StatusCode(500, "Internal server error");
        }
    }
}