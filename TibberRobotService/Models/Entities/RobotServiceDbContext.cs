using Microsoft.EntityFrameworkCore;

namespace TibberRobotService.Models.Entities;

public class RobotServiceDbContext: DbContext
{
    public RobotServiceDbContext(DbContextOptions<RobotServiceDbContext> options)
        : base(options)
    {
    }

    public DbSet<MovementSummary> MovementSummaries { get; set; }
}
