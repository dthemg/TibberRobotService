using TibberRobotService.Interfaces;
using TibberRobotService.Models;
using TibberRobotService.Utils;

namespace TibberRobotService.Services;

public class RobotMovementCalculatorService: IRobotMovementCalculatorService
{
    public long CalculateUniqueVisitedPositions(MovementRequest request)
    {
        // We are assuming we are immediately cleaning the starting square
        var uniquePositionsVisited = 1L;

        (var previousPosition, var traversedHorizontalLines, var traversedVerticalLines) = Initialize(request.Start);

        foreach (var command in request.Commands)
        {
            (var newLine, previousPosition) = RobotMovementUtils.ConstructLine(previousPosition, command);
            uniquePositionsVisited += ComputeNumberOfNewVisitedPositions(newLine, traversedHorizontalLines, traversedVerticalLines);
            AddToTraversedPaths(newLine, traversedHorizontalLines, traversedVerticalLines);
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

    private static void AddToTraversedPaths(
        Line newLine,
        Dictionary<int, List<Line1D>> traversedHorizontalPath,
        Dictionary<int, List<Line1D>> traversedVerticalPath)
    {
        if (newLine.IsHorizontal)
            AddToTraversedPath(newLine, traversedHorizontalPath);
        else
            AddToTraversedPath(newLine, traversedVerticalPath);
    }

    private static void AddToTraversedPath(
        Line newLine, Dictionary<int, List<Line1D>> traversedPath)
    {
        if (traversedPath.ContainsKey(newLine.Coordinate))
            traversedPath[newLine.Coordinate].Add(newLine.Span);
        else
            traversedPath[newLine.Coordinate] = new() { newLine.Span };
    }

    private static long ComputeNumberOfNewVisitedPositions(
        Line newLine,
        Dictionary<int, List<Line1D>> traversedHorizontalLines,
        Dictionary<int, List<Line1D>> traversedVerticalLines)
    {
        SortedDictionary<int, int> orderedCrossings;
        if (newLine.IsHorizontal)
        {
            orderedCrossings = LocateOverlapsAndCrossings(
                newLine.Span,
                newLine.Coordinate,
                traversedHorizontalLines,
                traversedVerticalLines);
        } else
        {
            orderedCrossings = LocateOverlapsAndCrossings(
                newLine.Span, 
                newLine.Coordinate,
                traversedVerticalLines,
                traversedHorizontalLines);
        }

        var numberOfOverlappingPoints = CountAlreadyTraversedPoints(newLine.Span, orderedCrossings);
        
        long pointsOnLine = newLine.Span.End - newLine.Span.Start + 1; // add 1 to include the starting point
        return pointsOnLine - numberOfOverlappingPoints;
    }

    private static SortedDictionary<int, int> LocateOverlapsAndCrossings(
        Line1D span,
        int coordinate,
        Dictionary<int, List<Line1D>> traversedParallelLines,
        Dictionary<int, List<Line1D>> traversedPerpendicularLines)
    {
        var orderedOverlaps = new SortedDictionary<int, int>();
        
        // Locate overlaps
        if (traversedParallelLines.TryGetValue(coordinate, out var potentialOverlaps))
        {
            foreach (var potentialOverlap in potentialOverlaps)
                CheckForOverlap(span, potentialOverlap, orderedOverlaps);
        }

        // Locate crossings
        foreach (var (crossing, perpendicularSpans) in traversedPerpendicularLines)
        {
            if (crossing < span.Start || crossing > span.End) continue;
            
            foreach (var perpendicularSpan in perpendicularSpans)
            {
                if (coordinate >= perpendicularSpan.Start && coordinate <= perpendicularSpan.End)
                    orderedOverlaps.TryAdd(span.Start, span.Start);
            }
        }
        return orderedOverlaps;
    }

    private static long CountAlreadyTraversedPoints(
        Line1D newLine,
        SortedDictionary<int, int> orderedOverlaps)
    {
        var numberOfOverlappingPoints = 0L;

        var positionToEvaluate = newLine.Start;
        foreach (var (startOfCrossing, endOfCrossing) in orderedOverlaps)
        {
            if (endOfCrossing < positionToEvaluate) continue;

            var startOfOverlap = Math.Max(positionToEvaluate, startOfCrossing);

            // Add 1 to include both ends of the overlap
            numberOfOverlappingPoints += Math.Min(endOfCrossing, newLine.End) - startOfOverlap + 1;
            positionToEvaluate = endOfCrossing + 1;
        }

        return numberOfOverlappingPoints;
    }

    private static void CheckForOverlap(Line1D newSpan, Line1D spanToCheck, SortedDictionary<int, int> orderedOverlaps)
    {
        var overlap = FindOverlap(newSpan, spanToCheck);
        if (overlap is null) return;

        if (orderedOverlaps.TryGetValue(overlap.Start, out var longestOverlap))
        {
            if (overlap.End > longestOverlap)
                orderedOverlaps[overlap.Start] = overlap.End;
        } else
            orderedOverlaps.Add(overlap.Start, overlap.End);
    }

    private static Line1D? FindOverlap(Line1D first, Line1D second)
    {
        // partially inside lower range
        if (second.Start <= first.Start && second.End >= first.Start && second.End <= first.End)
            return new Line1D(first.Start, second.End);

        // lineToUpdate completely inside range
        if (second.Start >= first.Start && second.End <= first.End)
            return new Line1D(second.Start, second.End);

        // partially inside upper range
        if (second.Start >= first.Start && second.Start <= first.End && second.End >= first.End)
            return new Line1D(second.Start, first.End);

        // range completely in lineToUpdate
        if (second.Start <= first.Start && second.End >= first.End)
            return new Line1D(first.Start, first.End);

        return null;
    }
}