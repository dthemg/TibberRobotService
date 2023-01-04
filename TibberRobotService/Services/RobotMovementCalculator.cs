using System.Diagnostics;
using TibberRobotService.Models;
using TibberRobotService.Utils;

namespace TibberRobotService.Services;

public static class RobotMovementCalculator
{
    private record Line1D(int Start, int End);

    public static int CalculateUniqueVisitedPositions(MovementRequest request)
    {
        // We are assuming we are immediately cleaning the starting square
        var uniquePositionsVisited = 1;

        (var previousPosition, var previousMovements) = Initialize(request.Start);

        foreach (var movement in request.Commands)
        {
            var newLine = RobotMovementUtils.ConstructLine(previousPosition, movement);
            uniquePositionsVisited += ComputeNumberOfNewVisitedPositions(newLine, previousMovements);
            previousPosition = RobotMovementUtils.FindPreviousPosition(newLine, movement);
            previousMovements.Add(newLine);
        }
        return uniquePositionsVisited;
    }

    private static (Position, List<Line>) Initialize(StartPosition start)
    {
        var initialMovement = new List<Line>() {
            new (start.X, start.Y, start.X, start.Y, true),
            new (start.X, start.Y, start.X, start.Y, false)
        };
        var initialPosition = new Position(start.X, start.Y);
        return (initialPosition, initialMovement);
    }

    private static int ComputeNumberOfNewVisitedPositions(Line line, List<Line> previousLines)
    {
        var orderedCrossings = LocateCrossingsAndOverlaps(line, previousLines);

        (int startOfLine, int endOfLine) = line.IsHorizontal ?
            (line.StartX, line.EndX) :
            (line.StartY, line.EndY);

        var intersections = 0;
        var positionToEvaluate = startOfLine;
        foreach (var pair in orderedCrossings)
        {
            var startOfCrossing = pair.Key;
            if (pair.Value is int endOfCrossing)
            {
                if (endOfCrossing < positionToEvaluate) continue;
                
                var startOfIntersection = Math.Max(positionToEvaluate, startOfCrossing);
                // Add 1 to include both ends of the overlap
                intersections += Math.Min(endOfCrossing, endOfLine) - startOfIntersection + 1; 
                positionToEvaluate = endOfCrossing + 1;
            }
            else
            {
                intersections++;
                positionToEvaluate++;
            }
        }

        // add 1 to include the starting point
        var pointsOnLine = line.EndX - line.StartX + line.EndY - line.StartY + 1;
        return pointsOnLine - intersections;
    }

    private static SortedDictionary<int, int?> LocateCrossingsAndOverlaps(Line movement, List<Line> previousMovement)
    {
        var orderedCrossings = new SortedDictionary<int, int?>();

        if (movement.IsHorizontal)
        {
            foreach (var line in previousMovement)
            {
                CheckHorizontalOverlap(movement, line, orderedCrossings);
                CheckHorizontalIntersection(movement, line, orderedCrossings);
            }
        }
        else
        {
            foreach (var line in previousMovement)
            {
                CheckVerticalOverlap(movement, line, orderedCrossings);
                CheckVerticalIntersection(movement, line, orderedCrossings);
            }
        }
        return orderedCrossings;
    }

    private static void CheckHorizontalOverlap(Line newMovement, Line lineToCheck, SortedDictionary<int, int?> crossings)
    {
        if (lineToCheck.IsHorizontal && newMovement.StartY == lineToCheck.StartY)
        {
            var overlap = OverlapInidices(newMovement.StartX, newMovement.EndX, lineToCheck.StartX, lineToCheck.EndX);
            if (overlap is null) return;
            UpdateDictionary(crossings, overlap);
        }
    }

    private static void CheckVerticalOverlap(Line newMovement, Line lineToCheck, SortedDictionary<int, int?> crossings)
    {
        if (!lineToCheck.IsHorizontal && newMovement.StartX == lineToCheck.StartX)
        {
            var overlap = OverlapInidices(newMovement.StartY, newMovement.EndY, lineToCheck.StartY, lineToCheck.EndY);
            if (overlap is null) return;
            UpdateDictionary(crossings, overlap);
        }
    }

    private static void UpdateDictionary(SortedDictionary<int, int?> crossings, Line1D lineToUpdate)
    {
        if (crossings.ContainsKey(lineToUpdate.Start))
        {
            var currentBest = crossings[lineToUpdate.Start];
            if (currentBest is null)
                crossings[lineToUpdate.Start] = lineToUpdate.End;
            else if (lineToUpdate.End > currentBest)
                crossings[lineToUpdate.Start] = lineToUpdate.End;
        }
        else
            crossings.Add(lineToUpdate.Start, lineToUpdate.End);
    }

    private static void CheckHorizontalIntersection(Line newMovement, Line lineToCheck, SortedDictionary<int, int?> crossings)
    {
        var intersectionPoint = CheckIntersection(newMovement, lineToCheck);
        if (intersectionPoint is not null)
            crossings.TryAdd(lineToCheck.StartX, null);
    }

    private static void CheckVerticalIntersection(Line newMovement, Line lineToCheck, SortedDictionary<int, int?> crossings)
    {
        var intersectionPoint = CheckIntersection(lineToCheck, newMovement);
        if (intersectionPoint != null)
            crossings.TryAdd(lineToCheck.StartY, null);
    }

    private static Position? CheckIntersection(Line horizontal, Line vertical)
    {
        if (horizontal.IsHorizontal && !vertical.IsHorizontal &&
            vertical.StartX >= horizontal.StartX &&
            vertical.StartX <= horizontal.EndX &&
            vertical.StartY <= horizontal.StartY &&
            vertical.EndY >= horizontal.StartY)
            return new Position(vertical.StartX, horizontal.StartY);
        else
            return null;
    }

    private static Line1D? OverlapInidices(int rangeStart, int rangeEnd, int overlapStart, int overlapEnd)
    {
        // partially inside lower range
        if (overlapStart <= rangeStart && overlapEnd >= rangeStart && overlapEnd <= rangeEnd)
            return new Line1D(rangeStart, overlapEnd);

        // lineToUpdate completely inside range
        if (overlapStart >= rangeStart && overlapEnd <= rangeEnd)
            return new Line1D(overlapStart, overlapEnd);

        // partially inside upper range
        if (overlapStart >= rangeStart && overlapStart <= rangeEnd && overlapEnd >= rangeEnd)
            return new Line1D(overlapStart, rangeEnd);

        // range completely in lineToUpdate
        if (overlapStart <= rangeStart && overlapEnd >= rangeEnd)
            return new Line1D(rangeStart, rangeEnd);

        return null;
    }
}
