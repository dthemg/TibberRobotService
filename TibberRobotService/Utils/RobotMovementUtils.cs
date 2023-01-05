using TibberRobotService.Models;
using TibberRobotService.Services;

namespace TibberRobotService.Utils;

public static class RobotMovementUtils
{
    /// <summary>
    /// Provided a starting position <paramref name="start"/> and a command given
    /// in <paramref name="command"/>, return a line descriptor, containing its span
    /// and coordinate, as well as the end position.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static (Line, Position) ConstructLine(Position start, Command command)
    {
        switch (command.Direction)
        {
            case Direction.East:
                var spanEast = new Line1D(start.X + 1, start.X + command.Steps);
                return (new Line(spanEast, start.Y, true), new(spanEast.End, start.Y));
            case Direction.North:
                var spanNorth = new Line1D(start.Y + 1, start.Y + command.Steps);
                return (new Line(spanNorth, start.X, false), new(start.X, spanNorth.End));
            case Direction.West:
                var spanWest = new Line1D(start.X - command.Steps, start.X - 1);
                return (new Line(spanWest, start.Y, true), new(spanWest.Start, start.Y));
            case Direction.South:
                var spanSouth = new Line1D(start.Y - command.Steps, start.Y - 1);
                return (new Line(spanSouth, start.X, false), new(start.X, spanSouth.Start));
            default:
                throw new ArgumentException($"Could not parse direction {command.Direction}", nameof(command));
        }
    }
}
