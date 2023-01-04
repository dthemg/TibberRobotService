namespace TibberRobotService.Services;

public record Line(int StartX, int StartY, int EndX, int EndY, bool IsHorizontal);

public record Position(int X, int Y);

