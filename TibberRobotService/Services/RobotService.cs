using System.Collections;
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

    private static int ComputeNumberOfNewVisitedPositions(Line newMovement, List<Line> previousMovement)
    {
        // add 1 to include the starting point
        var pointsOnLine = newMovement.EndX - newMovement.StartX + newMovement.EndY - newMovement.StartY + 1;

        int startIdx, endIdx;
        if (newMovement.IsHorizontal)
            (startIdx, endIdx) = (newMovement.StartX, newMovement.EndX);
        else
            (startIdx, endIdx) = (newMovement.StartY, newMovement.EndY);

        // instead store starting and stopping points, and then sort those badboys and we should be okay
        // only add new points?
        var intersections = new HashSet<int>();
        var overlaps = new List<Line1D>();

        foreach (var line in previousMovement)
        {
            if (newMovement.IsHorizontal)
            {
                CheckHorizontalOverlap(newMovement, line, overlaps);
                CheckHorizontalIntersection(newMovement, line, intersections);
            } else
            {
                CheckVerticalOverlap(newMovement, line, overlaps);
                CheckVerticalIntersection(newMovement, line, intersections);
            }
        }

        var alreadyTraversed = 0;
        var location = startIdx;


        // UPGRADE - sort by startIdx and then only iterate through those
        // OMG that should be an improvement!
        while (location <= endIdx)
        {
            var handled = false;
            foreach (var overlap in overlaps)
            {
                if (overlap.start <= location && overlap.end >= location)
                {
                    alreadyTraversed += Math.Min(overlap.end, endIdx) - location + 1;
                    location = overlap.end;
                    handled = true;
                    break;
                }
            }
            if (!handled && intersections.Contains(location))
            {
                alreadyTraversed++;
            }
            location++;
        }

        return pointsOnLine - alreadyTraversed;
    }

    private static void CheckHorizontalOverlap(Line newMovement, Line lineToCheck, List<Line1D> overlaps)
    {
        if (lineToCheck.IsHorizontal && newMovement.StartY == lineToCheck.StartY)
        {
            var overlap = OverlapInidices(newMovement.StartX, newMovement.EndX, lineToCheck.StartX, lineToCheck.EndX);
            if (overlap is null) return;
            overlaps.Add(overlap);
        }
    }

    private static void CheckVerticalOverlap(Line newMovement, Line lineToCheck, List<Line1D> overlaps)
    {
        if (!lineToCheck.IsHorizontal && newMovement.StartX == lineToCheck.StartX)
        {
            var overlap = OverlapInidices(newMovement.StartY, newMovement.EndY, lineToCheck.StartY, lineToCheck.EndY); ;
            if (overlap is null) return;
            overlaps.Add(overlap);
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

    private record Line1D(int start, int end);

    private static Line1D? OverlapInidices(int rangeStart, int rangeEnd, int overlapStart, int overlapEnd)
    {
        // partially inside lower range
        if (overlapStart <= rangeStart && overlapEnd >= rangeStart && overlapEnd <= rangeEnd)
            return new Line1D(rangeStart, overlapEnd);

        // overlap completely inside range
        if (overlapStart >= rangeStart && overlapEnd <= rangeEnd)
            return new Line1D(overlapStart, overlapEnd);

        // partially inside upper range
        if (overlapStart >= rangeStart && overlapStart <= rangeEnd && overlapEnd >= rangeEnd)
            return new Line1D(overlapStart, rangeEnd);

        // range completely in overlap
        if (overlapStart <= rangeStart && overlapEnd >= rangeEnd)
            return new Line1D(rangeStart, rangeEnd);

        return null;
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
