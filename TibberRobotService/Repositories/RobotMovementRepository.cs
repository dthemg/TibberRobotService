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

    public async Task<Db.Executions> AddMovementSummary(int numberOfCommands, int uniqueVisitedLocations, float calculationDuration)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var dbModel = ToDbModel(numberOfCommands, uniqueVisitedLocations, calculationDuration);

        Console.WriteLine("Storing to db");

        dbContext.MovementSummaries.Add(dbModel);

        // to fix - work a bit more on this!
        // await dbContext.SaveChangesAsync();
        dbModel.Id = Guid.NewGuid();

        return dbModel;
    }

    public async Task<IEnumerable<Db.Executions>> GetAllMovementSummaries()
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.MovementSummaries.ToListAsync();
    }

    private static Db.Executions ToDbModel(int numberOfCommands, int uniqueVisitedLocations, float calculationDuration)
    {
        return new()
        {
            Commands = numberOfCommands,
            Result = uniqueVisitedLocations,
            Duration = calculationDuration,
            Timestamp = DateTime.UtcNow
        };
    }
}
