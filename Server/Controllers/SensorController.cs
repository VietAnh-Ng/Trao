using Microsoft.AspNetCore.Mvc;
using TraoApp.Server.Services;

namespace TraoApp.Server.Controllers;

[Route("ws/[controller]")]
[ApiController]
public class SensorController : ControllerBase
{
    private readonly SensorService SensorService;

    public SensorController(SensorService _sensorService)
    {
        this.SensorService = _sensorService;
    }

    [Route("")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await this.SensorService.HandlerAsync(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}
