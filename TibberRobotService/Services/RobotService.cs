using TibberRobotService.Interfaces;
using TibberRobotService.Models;
using Db = TibberRobotService.Models.Entities;

namespace TibberRobotService.Services;

public class RobotService: IRobotService
{
    private record Line(int StartX, int StartY, int EndX, int EndY);
    private record Position(int X, int Y);

    private readonly IRobotMovementRepository _robotMovementRepository;

    public RobotService(IRobotMovementRepository robotMovementRepository)
    {
        _robotMovementRepository = robotMovementRepository;
    }

    public async Task<RobotMovementSummary> PerformRobotMovement(MovementRequest request)
    {
        var path = new List<Line>();

        var previousPosition = new Position(request.Start.X, request.Start.Y);
        foreach (var command in request.Commands)
        {
            Position newPosition = command.Direction switch
            {
                Direction.East => new Position(previousPosition.X + command.Steps, previousPosition.Y),
                Direction.North => new Position(previousPosition.X, previousPosition.Y + command.Steps),
                Direction.West => new Position(previousPosition.X - command.Steps, previousPosition.Y),
                Direction.South => new Position(previousPosition.X, previousPosition.Y - command.Steps),
                _ => throw new ArgumentException("Unable to parse direction", nameof(request)),
            };

            path.Add(new Line(previousPosition.X, previousPosition.Y, newPosition.X, newPosition.Y));
        }

        var uniqueVisitedLocations = ComputeNumberOfVisitedLocations(path);

        var duration = 0.001f;

        var addedEntity = await _robotMovementRepository.AddMovementSummary(
            request.Commands.Count, uniqueVisitedLocations, duration);

        return ToDto(addedEntity);
    }

    private static int ComputeNumberOfVisitedLocations(List<Line> movementPath)
    {
        // Strategy 1: add all steps, then remove intersections
        var numberOfVisitedLocations = 0;
        for (int i = 1; i < movementPath.Count; i++)
        {
            var line = movementPath[i];
            var lineDistance = Math.Abs(line.StartX - line.EndX) + Math.Abs(line.StartY - line.EndY);
            numberOfVisitedLocations += lineDistance;

            var lineHorizontal = line.StartX == line.EndX;

            for (int j = 0; j < i; j++)
            {
                var cross = movementPath[j];
                var crossingHorizontal = cross.StartX == cross.EndX;

                if (lineHorizontal)
                {

                }
            }
        }
        return 1;
    }

    private static RobotMovementSummary ToDto(Db.MovementSummary dbModel)
    {
        return new RobotMovementSummary(
            dbModel.Id,
            dbModel.Timestamp,
            dbModel.Commands,
            dbModel.Result,
            dbModel.Duration);
    }
}
