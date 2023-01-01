using Microsoft.AspNetCore.Mvc;
using TibberRobotService.Interfaces;
using TibberRobotService.Models;

namespace TibberRobotService.Controllers;

[ApiController]
[Produces("application/json")]
[Route("[controller]")]
public class RobotCommandController : ControllerBase
{
    private readonly IRobotService _robotService;

    public RobotCommandController(IRobotService robotService)
    {
        _robotService= robotService;
    }

    [HttpPost("/tibber-developer-test/enter-path")]
    public async Task<ActionResult<RobotMovementSummary>> MoveRobot([FromBody] MovementRequest movementRequest)
    {
        var executionSummary = await _robotService.PerformRobotMovement(movementRequest);

        return Ok(executionSummary);
    }
}