using TibberRobotService.Models;

namespace TibberRobotService.Interfaces;

public interface IRobotMovementCalculatorService
{
    public long CalculateUniqueVisitedPositions(MovementRequest request);
}
