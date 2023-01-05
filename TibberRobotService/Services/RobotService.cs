using System.Diagnostics;
using TibberRobotService.Interfaces;
using TibberRobotService.Models;
using Db = TibberRobotService.Models.Entities;
namespace TibberRobotService.Services;

public class RobotService: IRobotService
{
    private readonly IRobotMovementRepository _robotMovementRepository;

    public RobotService(IRobotMovementRepository robotMovementRepository)
    {
        _robotMovementRepository = robotMovementRepository;
    }

    public async Task<RobotMovementSummary> PerformRobotMovement(MovementRequest request)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var numberOfUniquePositions = RobotMovementCalculator.CalculateUniqueVisitedPositions(request);
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
