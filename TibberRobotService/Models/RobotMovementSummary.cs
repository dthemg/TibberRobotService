namespace TibberRobotService.Models;

public record RobotMovementSummary(
    Guid Id,
    DateTime Timestamp,
    int Commands,
    long Result,
    float Duration
);
