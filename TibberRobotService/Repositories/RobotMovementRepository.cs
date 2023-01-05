using Microsoft.EntityFrameworkCore;
using TibberRobotService.Interfaces;
using Db = TibberRobotService.Models.Entities;

namespace TibberRobotService.Repositories;

public class RobotMovementRepository : IRobotMovementRepository
{
    private readonly IDbContextFactory<Db.RobotServiceDbContext> _dbContextFactory;

    public RobotMovementRepository(IDbContextFactory<Db.RobotServiceDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Db.Executions> AddMovementSummary(int numberOfCommands, long uniqueVisitedLocations, float calculationDuration)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var dbModel = ToDbModel(numberOfCommands, uniqueVisitedLocations, calculationDuration);

        dbContext.executions.Add(dbModel);

        await dbContext.SaveChangesAsync();

        return dbModel;
    }

    public async Task<IEnumerable<Db.Executions>> GetAllMovementSummaries()
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.executions.ToListAsync();
    }

    private static Db.Executions ToDbModel(
        int numberOfCommands,
        long uniqueVisitedLocations,
        float calculationDuration)
    {
        return new()
        {
            commands = numberOfCommands,
            result = uniqueVisitedLocations,
            duration = calculationDuration,
            timestamp = DateTime.UtcNow
        };
    }
}
