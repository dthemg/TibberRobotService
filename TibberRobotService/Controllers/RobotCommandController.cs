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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RobotMovementSummary>> MoveRobot([FromBody] MovementRequest movementRequest)
    {
        try
        {
            var executionSummary = await _robotService.PerformRobotMovement(movementRequest);
            return Ok(executionSummary);
        } catch (ArgumentException e) 
        {
            return BadRequest(e.Message);
        }
    }
}