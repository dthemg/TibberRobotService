using TibberRobotService.Models;
using TibberRobotService.Services;

namespace TibberRobotService.Utils;

public static class RobotMovementUtils
{
    /// <summary>
    /// Provided a starting position <paramref name="start"/> and a movement given
    /// in <paramref name="movement"/>, return a line containing all points in 
    /// the movement path excluding the starting position
    /// </summary>
    /// <param name="start"></param>
    /// <param name="movement"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Line ConstructLine(Position start, Movement movement)
    {
        return movement.Direction switch
        {
            Direction.East =>
                new Line(start.X + 1, start.Y, start.X + movement.Steps, start.Y, true),
            Direction.North =>
                new Line(start.X, start.Y + 1, start.X, start.Y + movement.Steps, false),
            Direction.West =>
                new Line(start.X - movement.Steps, start.Y, start.X - 1, start.Y, true),
            Direction.South =>
                new Line(start.X, start.Y - movement.Steps, start.X, start.Y - 1, false),
            _ => throw new ArgumentException("Unable to parse direction", nameof(movement))
        };
    }

    /// <summary>
    /// Find the position of the end of a line <paramref name="line"/> produced by movement <paramref name="movement"/>
    /// </summary>
    /// <param name="line"></param>
    /// <param name="movement"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Position FindPreviousPosition(Line line, Movement movement)
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
}
