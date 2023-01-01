namespace TibberRobotService.Models;

#nullable disable

public class MovementRequest
{
    public StartPosition Start { get; set; }
    public List<Movement> Commands { get; set; } = new();
}

public class StartPosition
{
    public int X { get; set; }
    public int Y { get; set; }
}

public class Movement
{
    public Direction Direction { get; set; }
    public int Steps { get; set; }
}

public enum Direction
{
    East = 0,
    West = 1,
    North = 2,
    South = 3
}
