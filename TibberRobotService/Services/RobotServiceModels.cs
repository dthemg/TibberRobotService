namespace TibberRobotService.Services;

public record Line(Line1D Span, int Coordinate, bool IsHorizontal);

public record Line1D(int Start, int End);

public record Position(int X, int Y);

