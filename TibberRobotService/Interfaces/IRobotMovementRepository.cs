using Db = TibberRobotService.Models.Entities;

namespace TibberRobotService.Interfaces;

public interface IRobotMovementRepository
{
    Task<Db.Executions> AddMovementSummary(
        int numberOfCommands,
        int uniqueVisitedLocations,
        float calculationDuration);

    Task<IEnumerable<Db.Executions>> GetAllMovementSummaries();
}
