namespace TibberRobotService.Models;

public record RobotMovementSummary(
    Guid Id,
    DateTime Timestamp,
    int Commands,
    int Result,
    float Duration
);
