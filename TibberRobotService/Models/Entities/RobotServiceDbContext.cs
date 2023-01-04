using Microsoft.EntityFrameworkCore;

namespace TibberRobotService.Models.Entities;

public class RobotServiceDbContext: DbContext
{
    public RobotServiceDbContext(DbContextOptions<RobotServiceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Executions> executions { get; set; }
}
