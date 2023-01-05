namespace TibberRobotService.Services;

public record Line(int StartX, int StartY, int EndX, int EndY, bool IsHorizontal);

public record Line1D(int Start, int End);

public record Position(int X, int Y);

