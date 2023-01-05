using TibberRobotService.Models;
using TibberRobotService.Services;

namespace TibberRobotService.Utils;

public static class RobotMovementUtils
{
    /// <summary>
    /// Provided a starting position <paramref name="start"/> and a command given
    /// in <paramref name="command"/>, return a line containing all points in 
    /// the command path excluding the starting position
    /// </summary>
    /// <param name="start"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Services.Line ConstructLine(Position start, Command command)
    {
        return command.Direction switch
        {
            Direction.East =>
                new Services.Line(start.X + 1, start.Y, start.X + command.Steps, start.Y, true),
            Direction.North =>
                new Services.Line(start.X, start.Y + 1, start.X, start.Y + command.Steps, false),
            Direction.West =>
                new Services.Line(start.X - command.Steps, start.Y, start.X - 1, start.Y, true),
            Direction.South =>
                new Services.Line(start.X, start.Y - command.Steps, start.X, start.Y - 1, false),
            _ => throw new ArgumentException("Unable to parse direction", nameof(command))
        };
    }

    /// <summary>
    /// Find the position of the end of a line <paramref name="line"/> produced by command <paramref name="movement"/>
    /// </summary>
    /// <param name="line"></param>
    /// <param name="movement"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Position FindPreviousPosition(Services.Line line, Command movement)
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
