﻿using TibberRobotService.Interfaces;
using TibberRobotService.Models.Entities;

namespace TestPerformance;

public class RepoMock : IRobotMovementRepository
{
    public async Task<Executions> AddMovementSummary(int numberOfCommands, int uniqueVisitedLocations, float calculationDuration)
    {
        await Task.Delay(1);
        return new()
        {
            Commands = numberOfCommands,
            Result = uniqueVisitedLocations,
            Duration = calculationDuration
        };
    }

    public Task<IEnumerable<Executions>> GetAllMovementSummaries()
    {
        throw new NotImplementedException();
    }
}