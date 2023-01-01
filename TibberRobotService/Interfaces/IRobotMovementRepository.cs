using Db = TibberRobotService.Models.Entities;

namespace TibberRobotService.Interfaces;

public interface IRobotMovementRepository
{
    Task<Db.MovementSummary> AddMovementSummary(
        int numberOfCommands,
        int uniqueVisitedLocations,
        float calculationDuration);

    Task<IEnumerable<Db.MovementSummary>> GetAllMovementSummaries();
}
