using TibberRobotService.Models;
using TibberRobotService.Utils;

namespace TibberRobotService.Services;

public static class RobotMovementCalculator
{
    

    public static int CalculateUniqueVisitedPositions(MovementRequest request)
    {
        // We are assuming we are immediately cleaning the starting square
        var uniquePositionsVisited = 1;

        (var previousPosition, var traversedHorizontalLines, var traversedVerticalLines) = Initialize(request.Start);

        foreach (var command in request.Commands)
        {
            var newLine = RobotMovementUtils.ConstructLine(previousPosition, command);
            uniquePositionsVisited += ComputeNumberOfNewVisitedPositions(newLine, traversedHorizontalLines, traversedVerticalLines);
            previousPosition = RobotMovementUtils.FindPreviousPosition(newLine, command);
            if (newLine.IsHorizontal)
            {
                if (traversedHorizontalLines.ContainsKey(newLine.StartY))
                    traversedHorizontalLines[newLine.StartY].Add(new(newLine.StartX, newLine.EndX));
                else
                    traversedHorizontalLines[newLine.StartY] = new() { new(newLine.StartX, newLine.EndX) };
            } else
            {
                if (traversedVerticalLines.ContainsKey(newLine.StartX))
                    traversedVerticalLines[newLine.StartX].Add(new(newLine.StartY, newLine.EndY));
                else
                    traversedVerticalLines[newLine.StartX] = new() { new(newLine.StartY, newLine.EndY) };
            }
        }
        return uniquePositionsVisited;
    }

    private static (Position, Dictionary<int, List<Line1D>>, Dictionary<int, List<Line1D>>) Initialize(StartPosition start)
    {
        var initialHorizontalLines = new Dictionary<int, List<Line1D>>
        {
            { start.Y, new() { new Line1D(start.X, start.X) } }
        };
        var initialVerticalLines = new Dictionary<int, List<Line1D>>
        {
            { start.X, new() { new Line1D(start.Y, start.Y) } }
        };

        var initialPosition = new Position(start.X, start.Y);
        return (initialPosition, initialHorizontalLines, initialVerticalLines);
    }

    private static int ComputeNumberOfNewVisitedPositions(Line newLine, Dictionary<int, List<Line1D>> traversedHorizontalLines, Dictionary<int, List<Line1D>> traversedVerticalLines)
    {
        Line1D newLine1D;

        SortedDictionary<int, int> orderedCrossings;
        if (newLine.IsHorizontal)
        {
            newLine1D = new(newLine.StartX, newLine.EndX);
            orderedCrossings = LocateCrossingsAndOverlaps(
                new Line1D(newLine.StartX, newLine.EndX),
                newLine.StartY,
                traversedHorizontalLines,
                traversedVerticalLines);
        } else
        {
            newLine1D = new(newLine.StartY, newLine.EndY);
            orderedCrossings = LocateCrossingsAndOverlaps(
                new Line1D(newLine.StartY, newLine.EndY), 
                newLine.StartX,
                traversedVerticalLines,
                traversedHorizontalLines);
        }
        var numberOfOverlappingPoints = CountOverlappingPoints(newLine1D, orderedCrossings);

        // add 1 to include the starting point
        var pointsOnLine = newLine.EndX - newLine.StartX + newLine.EndY - newLine.StartY + 1;
        return pointsOnLine - numberOfOverlappingPoints;
    }

    private static SortedDictionary<int, int> LocateCrossingsAndOverlaps(
        Line1D movement,
        int Coordinate,
        Dictionary<int, List<Line1D>> traversedParallelLines,
        Dictionary<int, List<Line1D>> traversedPerpendicularLines)
    {
        var orderedOverlaps = new SortedDictionary<int, int>();
        if (traversedParallelLines.TryGetValue(Coordinate, out var potentialOverlaps))
        {
            foreach (var potentialOverlap in potentialOverlaps)
                CheckForOverlap(movement.Start, movement.End, potentialOverlap.Start, potentialOverlap.End, orderedOverlaps);
        }

        foreach (var potentialCrossing in traversedPerpendicularLines)
        {
            var crossings = potentialCrossing.Key;
            if (crossings < movement.Start || crossings > movement.End) continue;
            
            var line1Ds = potentialCrossing.Value;
            foreach (var line1D in line1Ds)
            {
                if (Coordinate >= line1D.Start && Coordinate <= line1D.End)
                    orderedOverlaps.TryAdd(movement.Start, movement.Start);
            }
        }
        return orderedOverlaps;
    }

    private static int CountOverlappingPoints(Line1D newLine, SortedDictionary<int, int> orderedOverlaps)
    {
        var numberOfOverlappingPoints = 0;

        var positionToEvaluate = newLine.Start;
        foreach (var pair in orderedOverlaps)
        {
            var startOfCrossing = pair.Key;
            var endOfCrossing = pair.Value;

            if (endOfCrossing < positionToEvaluate) continue;

            var startOfOverlap = Math.Max(positionToEvaluate, startOfCrossing);
            
            // Add 1 to include both ends of the overlap
            numberOfOverlappingPoints += Math.Min(endOfCrossing, newLine.End) - startOfOverlap + 1;
            positionToEvaluate = endOfCrossing + 1;
        }

        return numberOfOverlappingPoints;
    }

    private static void CheckForOverlap(int start, int end, int toCheckStart, int toCheckEnd, SortedDictionary<int, int> orderedOverlaps)
    {
        var overlap = FindOverlap(start, end, toCheckStart, toCheckEnd);
        if (overlap is null) return;

        if (orderedOverlaps.TryGetValue(overlap.Start, out var longestOverlap))
        {
            if (overlap.End > longestOverlap)
                orderedOverlaps[overlap.Start] = overlap.End;
        } else
            orderedOverlaps.Add(overlap.Start, overlap.End);
    }

    private static Line1D? FindOverlap(int lineStart, int lineEnd, int toCheckStart, int toCheckEnd)
    {
        // partially inside lower range
        if (toCheckStart <= lineStart && toCheckEnd >= lineStart && toCheckEnd <= lineEnd)
            return new Line1D(lineStart, toCheckEnd);

        // lineToUpdate completely inside range
        if (toCheckStart >= lineStart && toCheckEnd <= lineEnd)
            return new Line1D(toCheckStart, toCheckEnd);

        // partially inside upper range
        if (toCheckStart >= lineStart && toCheckStart <= lineEnd && toCheckEnd >= lineEnd)
            return new Line1D(toCheckStart, lineEnd);

        // range completely in lineToUpdate
        if (toCheckStart <= lineStart && toCheckEnd >= lineEnd)
            return new Line1D(lineStart, lineEnd);

        return null;
    }
}