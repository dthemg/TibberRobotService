using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.Security.AccessControl;
using TibberRobotService.Models;
using TibberRobotService.Utils;

namespace TibberRobotService.Services;

public static class RobotMovementCalculator
{
    private record Path1D(int Start, int End);

    public static int CalculateUniqueVisitedPositions(MovementRequest request)
    {
        // We are assuming we are immediately cleaning the starting square
        var uniquePositionsVisited = 1;

        (var previousPosition, var traversedLines) = Initialize(request.Start);

        foreach (var command in request.Commands)
        {
            var newLine = RobotMovementUtils.ConstructLine(previousPosition, command);
            uniquePositionsVisited += ComputeNumberOfNewVisitedPositions(newLine, traversedLines);
            previousPosition = RobotMovementUtils.FindPreviousPosition(newLine, command);
            traversedLines.Add(newLine);
        }
        return uniquePositionsVisited;
    }

    private static (Position, List<Line>) Initialize(StartPosition start)
    {
        var initialLine = new List<Line>() {
            new (start.X, start.Y, start.X, start.Y, true),
            new (start.X, start.Y, start.X, start.Y, false)
        };
        var initialPosition = new Position(start.X, start.Y);
        return (initialPosition, initialLine);
    }

    private static int ComputeNumberOfNewVisitedPositions(Line newLine, List<Line> previousLines)
    {
        var orderedCrossings = LocateCrossingsAndOverlaps(newLine, previousLines);
        var numberOfOverlappingPoints = CountOverlappingPoints(newLine, orderedCrossings);

        // add 1 to include the starting point
        var pointsOnLine = newLine.EndX - newLine.StartX + newLine.EndY - newLine.StartY + 1;
        return pointsOnLine - numberOfOverlappingPoints;
    }

    private static SortedDictionary<int, int> LocateCrossingsAndOverlaps(Line movement, List<Line> traversedLines)
    {
        var orderedOverlaps = new SortedDictionary<int, int>();

        if (movement.IsHorizontal)
        {
            foreach (var traversedLine in traversedLines)
            {
                CheckForHorizontalOverlap(movement, traversedLine, orderedOverlaps);
                CheckForHorizontalIntersection(movement, traversedLine, orderedOverlaps);
            }
        }
        else
        {
            foreach (var traversedLine in traversedLines)
            {
                CheckForVerticalOverlap(movement, traversedLine, orderedOverlaps);
                CheckForVerticalIntersection(movement, traversedLine, orderedOverlaps);
            }
        }
        return orderedOverlaps;
    }

    private static int CountOverlappingPoints(Line newLine, SortedDictionary<int, int> orderedOverlaps)
    {
        (int startOfLine, int endOfLine) = newLine.IsHorizontal ?
            (newLine.StartX, newLine.EndX) :
            (newLine.StartY, newLine.EndY);

        var numberOfOverlappingPoints = 0;
        var positionToEvaluate = startOfLine;
        
        foreach (var pair in orderedOverlaps)
        {
            var startOfCrossing = pair.Key;
            var endOfCrossing = pair.Value;

            if (endOfCrossing < positionToEvaluate) continue;

            var startOfOverlap = Math.Max(positionToEvaluate, startOfCrossing);
            
            // Add 1 to include both ends of the overlap
            numberOfOverlappingPoints += Math.Min(endOfCrossing, endOfLine) - startOfOverlap + 1;
            positionToEvaluate = endOfCrossing + 1;
        }

        return numberOfOverlappingPoints;
    }

    private static void CheckForHorizontalOverlap(Line newLine, Line lineToCheck, SortedDictionary<int, int> orderedOverlaps)
    {
        if (lineToCheck.IsHorizontal && newLine.StartY == lineToCheck.StartY)
        {
            var overlap = FindOverlap(newLine.StartX, newLine.EndX, lineToCheck.StartX, lineToCheck.EndX);
            if (overlap is null) return;
            UpdateDictionary(orderedOverlaps, overlap);
        }
    }

    private static void CheckForVerticalOverlap(Line newLine, Line lineToCheck, SortedDictionary<int, int> orderedOverlaps)
    {
        if (!lineToCheck.IsHorizontal && newLine.StartX == lineToCheck.StartX)
        {
            var overlap = FindOverlap(newLine.StartY, newLine.EndY, lineToCheck.StartY, lineToCheck.EndY);
            if (overlap is null) return;
            UpdateDictionary(orderedOverlaps, overlap);
        }
    }

    private static void UpdateDictionary(SortedDictionary<int, int> crossings, Path1D lineToUpdate)
    {
        if (crossings.ContainsKey(lineToUpdate.Start))
        {
            var currentBest = crossings[lineToUpdate.Start];
            if (lineToUpdate.End > currentBest)
                crossings[lineToUpdate.Start] = lineToUpdate.End;
        }
        else
            crossings.Add(lineToUpdate.Start, lineToUpdate.End);
    }

    private static void CheckForHorizontalIntersection(Line newLine, Line lineToCheck, SortedDictionary<int, int> orderedOverlaps)
    {
        var intersectionPoint = CheckIntersection(newLine, lineToCheck);
        if (intersectionPoint is not null)
            orderedOverlaps.TryAdd(lineToCheck.StartX, lineToCheck.StartX);
    }

    private static void CheckForVerticalIntersection(Line newLine, Line lineToCheck, SortedDictionary<int, int> orderedOverlaps)
    {
        var intersectionPoint = CheckIntersection(lineToCheck, newLine);
        if (intersectionPoint != null)
            orderedOverlaps.TryAdd(lineToCheck.StartY, lineToCheck.StartY);
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

    private static Path1D? FindOverlap(int lineStart, int lineEnd, int toCheckStart, int toCheckEnd)
    {
        // partially inside lower range
        if (toCheckStart <= lineStart && toCheckEnd >= lineStart && toCheckEnd <= lineEnd)
            return new Path1D(lineStart, toCheckEnd);

        // lineToUpdate completely inside range
        if (toCheckStart >= lineStart && toCheckEnd <= lineEnd)
            return new Path1D(toCheckStart, toCheckEnd);

        // partially inside upper range
        if (toCheckStart >= lineStart && toCheckStart <= lineEnd && toCheckEnd >= lineEnd)
            return new Path1D(toCheckStart, lineEnd);

        // range completely in lineToUpdate
        if (toCheckStart <= lineStart && toCheckEnd >= lineEnd)
            return new Path1D(lineStart, lineEnd);

        return null;
    }
}
