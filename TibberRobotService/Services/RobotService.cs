using TibberRobotService.Interfaces;
using TibberRobotService.Models;
using Db = TibberRobotService.Models.Entities;

namespace TibberRobotService.Services;

public class RobotService: IRobotService
{
    private record Line(int StartX, int StartY, int EndX, int EndY, bool isHorizontal);
    private record Position(int X, int Y);

    private readonly IRobotMovementRepository _robotMovementRepository;

    public RobotService(IRobotMovementRepository robotMovementRepository)
    {
        _robotMovementRepository = robotMovementRepository;
    }

    public async Task<RobotMovementSummary> PerformRobotMovement(MovementRequest request)
    {
        // We are assuming we are cleaning the starting square
        var movement = new List<Line>() { 
            new (request.Start.X, request.Start.Y, request.Start.X, request.Start.Y, true),
            new (request.Start.X, request.Start.Y, request.Start.X, request.Start.Y, false)
        };
        var uniquePositionsVisited = 1;

        var previousPosition = new Position(request.Start.X, request.Start.Y);
        foreach (var command in request.Commands)
        {
            Line newLine = command.Direction switch
            {
                Direction.East => 
                    new Line(previousPosition.X + 1, previousPosition.Y, previousPosition.X + command.Steps, previousPosition.Y, true),
                Direction.North => 
                    new Line(previousPosition.X, previousPosition.Y + 1, previousPosition.X, previousPosition.Y + command.Steps, false),
                Direction.West => 
                    new Line(previousPosition.X - command.Steps, previousPosition.Y, previousPosition.X - 1, previousPosition.Y, true),
                Direction.South => 
                    new Line(previousPosition.X, previousPosition.Y - command.Steps, previousPosition.X, previousPosition.Y - 1, false),
                _ => throw new ArgumentException("Unable to parse direction", nameof(request))
            };

            var newPositions = ComputeNumberOfNewVisitedPositions(newLine, movement);
            uniquePositionsVisited += newPositions;

            previousPosition = command.Direction switch
            {
                Direction.East => new Position(newLine.EndX, newLine.EndY),
                Direction.North => new Position(newLine.EndX, newLine.EndY),
                Direction.West => new Position(newLine.StartX, newLine.StartY),
                Direction.South => new Position(newLine.StartX, newLine.StartY),
                _ => throw new ArgumentException("Unable to parse direction", nameof(request))
            };
            movement.Add(newLine);
        }

        var duration = 0.001f;

        var addedEntity = await _robotMovementRepository.AddMovementSummary(
            request.Commands.Count, uniquePositionsVisited, duration);

        return ToDto(addedEntity);
    }

    private static int ComputeNumberOfNewVisitedPositions(Line newMovement, List<Line> previousMovement)
    {
        var intersectionPoints = new HashSet<Position>();
        
        var undisturbedMovement = newMovement.EndX - newMovement.StartX + newMovement.EndY - newMovement.StartY;

        foreach (var line in previousMovement)
        {
            if (newMovement.isHorizontal && line.isHorizontal && newMovement.StartY == line.StartY)
            {
                var overlap = OverlapInidices(newMovement.StartX, newMovement.EndX, line.StartX, line.EndX)
                    .Select(x => new Position(x, line.StartY));
                intersectionPoints.UnionWith(overlap);
            } else if (!newMovement.isHorizontal && !line.isHorizontal && newMovement.StartX == line.StartX)
            {
                var overlap = OverlapInidices(newMovement.StartY, newMovement.EndY, line.StartY, line.EndY)
                    .Select(x => new Position(line.StartX, x));
                intersectionPoints.UnionWith(overlap);
            } else if (newMovement.isHorizontal && !line.isHorizontal)
            {
                if (line.StartX >= newMovement.StartX &&
                    line.StartX <= newMovement.EndX &&
                    line.StartY <= newMovement.StartY &&
                    line.EndY >= newMovement.StartY)
                {
                    intersectionPoints.Add(new Position(line.StartX, newMovement.StartY));
                    Console.WriteLine($"Intersection point at ({line.StartX}, {newMovement.StartY})");
                }
            } else if (!newMovement.isHorizontal && line.isHorizontal)
            {
                if (line.StartX <= newMovement.StartX &&
                    line.EndX >= newMovement.StartX &&
                    line.StartY >= newMovement.StartY &&
                    line.StartY <= newMovement.EndY)
                {
                    intersectionPoints.Add(new Position(newMovement.StartX, line.StartY));
                    Console.WriteLine($"Intersection point at ({newMovement.StartX}, {line.StartY})");
                }
            }
        }

        // add 1 to include both edges
        var toAdd = undisturbedMovement - intersectionPoints.Count + 1;
        return toAdd;
    }

    private static IEnumerable<int> OverlapInidices(int rangeStart, int rangeEnd, int overlapStart, int overlapEnd)
    {
        var emptyReturn = Enumerable.Empty<int>();

        // below everything
        if (overlapStart < rangeStart && overlapEnd < rangeStart)
            return emptyReturn;

        // above everything
        if (overlapStart > rangeEnd && overlapEnd > rangeEnd)
            return emptyReturn;

        // partially inside lower range
        if (overlapStart <= rangeStart && overlapEnd >= rangeStart && overlapEnd <= rangeEnd)
            return Range(rangeStart, overlapEnd);

        // overlap completely inside range
        if (overlapStart >= rangeStart && overlapEnd <= rangeEnd)
            return Range(overlapStart, overlapEnd);

        // partially inside upper range
        if (overlapStart >= rangeStart && overlapStart <= rangeEnd && overlapEnd >= rangeEnd)
            return Range(overlapStart, rangeEnd);

        // range completely in overlap
        if (overlapStart <= rangeStart && overlapEnd >= rangeEnd)
            return Range(rangeStart, rangeEnd);

        return emptyReturn;

    }

    private static IEnumerable<int> Range(int start, int end) => Enumerable.Range(start, end - start + 1);
    
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
