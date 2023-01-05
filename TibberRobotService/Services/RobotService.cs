using System.Diagnostics;
using TibberRobotService.Interfaces;
using TibberRobotService.Models;
using Db = TibberRobotService.Models.Entities;
namespace TibberRobotService.Services;

public class RobotService: IRobotService
{
    private readonly IRobotMovementRepository _robotMovementRepository;
    private readonly IRobotMovementCalculatorService _robotMovementCalculator;

    public RobotService(IRobotMovementRepository robotMovementRepository, IRobotMovementCalculatorService robotMovementCalculator)
    {
        _robotMovementRepository = robotMovementRepository;
        _robotMovementCalculator = robotMovementCalculator;
    }

    public async Task<RobotMovementSummary> PerformRobotMovement(MovementRequest request)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var numberOfUniquePositions = _robotMovementCalculator.CalculateUniqueVisitedPositions(request);
        stopwatch.Stop();
        
        var calculationDuration = stopwatch.ElapsedTicks / (float)TimeSpan.TicksPerSecond;
        
        var addedEntity = await _robotMovementRepository.AddMovementSummary(
            request.Commands.Count, numberOfUniquePositions, calculationDuration);
        return ToDto(addedEntity);
    }

    private static RobotMovementSummary ToDto(Db.Executions dbModel)
    {
        return new RobotMovementSummary(
            dbModel.id,
            dbModel.timestamp,
            dbModel.commands,
            dbModel.result,
            dbModel.duration);
    }
}
