using TibberRobotService.Models;

namespace TibberRobotService.Interfaces;

public interface IRobotService
{
    Task<RobotMovementSummary> PerformRobotMovement(MovementRequest request);
}
