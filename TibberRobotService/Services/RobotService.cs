using System.Diagnostics;
using TibberRobotService.Interfaces;
using TibberRobotService.Models;
using Db = TibberRobotService.Models.Entities;
namespace TibberRobotService.Services;

public class RobotService: IRobotService
{
    private record Line(int StartX, int StartY, int EndX, int EndY, bool IsHorizontal);
    private record Position(int X, int Y);

    private readonly IRobotMovementRepository _robotMovementRepository;

    public RobotService(IRobotMovementRepository robotMovementRepository)
    {
        _robotMovementRepository = robotMovementRepository;
    }

    public async Task<RobotMovementSummary> PerformRobotMovement(MovementRequest request)
    {
        var previousMovements = new List<Line>() { 
            new (request.Start.X, request.Start.Y, request.Start.X, request.Start.Y, true),
            new (request.Start.X, request.Start.Y, request.Start.X, request.Start.Y, false)
        };

        // We are assuming we are immediately cleaning the starting square
        var uniquePositionsVisited = 1;
        var previousPosition = new Position(request.Start.X, request.Start.Y);

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        foreach (var movement in request.Commands)
        {
            var newLine = ConstructLine(previousPosition, movement);
            uniquePositionsVisited += ComputeNumberOfNewVisitedPositions(newLine, previousMovements);
            previousPosition = UpdatePreviousPosition(newLine, movement);
            previousMovements.Add(newLine);
        }
        stopwatch.Stop();
        var duration = stopwatch.ElapsedTicks / (float)TimeSpan.TicksPerSecond;
        Console.WriteLine(duration);
        var addedEntity = await _robotMovementRepository.AddMovementSummary(
            request.Commands.Count, uniquePositionsVisited, duration);

        return ToDto(addedEntity);
    }

    private static Line ConstructLine(Position previousPosition, Movement movement)
    {
        return movement.Direction switch
        {
            Direction.East =>
                new Line(previousPosition.X + 1, previousPosition.Y, previousPosition.X + movement.Steps, previousPosition.Y, true),
            Direction.North =>
                new Line(previousPosition.X, previousPosition.Y + 1, previousPosition.X, previousPosition.Y + movement.Steps, false),
            Direction.West =>
                new Line(previousPosition.X - movement.Steps, previousPosition.Y, previousPosition.X - 1, previousPosition.Y, true),
            Direction.South =>
                new Line(previousPosition.X, previousPosition.Y - movement.Steps, previousPosition.X, previousPosition.Y - 1, false),
            _ => throw new ArgumentException("Unable to parse direction", nameof(movement))
        };
    }

    private static Position UpdatePreviousPosition(Line line, Movement movement)
    {
        return movement.Direction switch
        {
            Direction.East => new Position(line.EndX, line.EndY),
            Direction.North => new Position(line.EndX, line.EndY),
            Direction.West => new Position(line.StartX, line.StartY),
            Direction.South => new Position(line.StartX, line.StartY),
            _ => throw new ArgumentException("Unable to parse direction", nameof(movement))
        };
    }

    // private record Line1D(int start, int stop);

    private static int ComputeNumberOfNewVisitedPositions(Line newMovement, List<Line> previousMovement)
    {
        // add 1 to include the starting point
        var pointsOnLine = newMovement.EndX - newMovement.StartX + newMovement.EndY - newMovement.StartY + 1;

        // instead store starting and stopping points, and then sort those badboys and we should be okay
        var intersectionPoints = new HashSet<int>();
        foreach (var line in previousMovement)
        {
            if (newMovement.IsHorizontal)
            {
                CheckHorizontalOverlap(newMovement, line, intersectionPoints);
                CheckHorizontalIntersection(newMovement, line, intersectionPoints);
            } else
            {
                CheckVerticalOverlap(newMovement, line, intersectionPoints);
                CheckVerticalIntersection(newMovement, line, intersectionPoints);
            }
        }
        return pointsOnLine - intersectionPoints.Count;
    }

    private static void CheckHorizontalOverlap(Line newMovement, Line lineToCheck, HashSet<int> intersectionPoints)
    {
        if (lineToCheck.IsHorizontal && newMovement.StartY == lineToCheck.StartY)
        {
            var overlap = OverlapInidices(newMovement.StartX, newMovement.EndX, lineToCheck.StartX, lineToCheck.EndX);
            intersectionPoints.UnionWith(overlap);
        }
    }

    private static void CheckVerticalOverlap(Line newMovement, Line lineToCheck, HashSet<int> intersectionPoints)
    {
        if (!lineToCheck.IsHorizontal && newMovement.StartX == lineToCheck.StartX)
        {
            var overlap = OverlapInidices(newMovement.StartY, newMovement.EndY, lineToCheck.StartY, lineToCheck.EndY); ;
            intersectionPoints.UnionWith(overlap);
        }
    }

    private static void CheckHorizontalIntersection(Line newMovement, Line lineToCheck, HashSet<int> intersectionPoints)
    {
        if (!lineToCheck.IsHorizontal &&
            lineToCheck.StartX >= newMovement.StartX &&
            lineToCheck.StartX <= newMovement.EndX &&
            lineToCheck.StartY <= newMovement.StartY &&
            lineToCheck.EndY >= newMovement.StartY)
        { 
            intersectionPoints.Add(lineToCheck.StartX);
        }
    }

    private static void CheckVerticalIntersection(Line newMovement, Line lineToCheck, HashSet<int> intersectionPoints)
    {
        if (lineToCheck.IsHorizontal &&
            lineToCheck.StartX <= newMovement.StartX &&
            lineToCheck.EndX >= newMovement.StartX &&
            lineToCheck.StartY >= newMovement.StartY &&
            lineToCheck.StartY <= newMovement.EndY)
        {
            intersectionPoints.Add(lineToCheck.StartY);
        }
    }

    private static IEnumerable<int> OverlapInidices(int rangeStart, int rangeEnd, int overlapStart, int overlapEnd)
    {
        var emptyReturn = Enumerable.Empty<int>();

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
    
    private static RobotMovementSummary ToDto(Db.Executions dbModel)
    {
        return new RobotMovementSummary(
            dbModel.Id,
            dbModel.Timestamp,
            dbModel.Commands,
            dbModel.Result,
            dbModel.Duration);
    }
}
