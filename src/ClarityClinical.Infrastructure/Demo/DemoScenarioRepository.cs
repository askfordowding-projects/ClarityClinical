using ClarityClinical.Application.Demo;
using ClarityClinical.Domain.Demo;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClarityClinical.Infrastructure.Demo;

public sealed class DemoScenarioRepository(ClarityClinicalDbContext dbContext) : IDemoScenarioRepository
{
    public Task<DemoScenario?> GetCanonicalScenarioAsync(
        string scenarioKey,
        CancellationToken cancellationToken)
    {
        return dbContext.DemoScenarios
            .Include(scenario => scenario.Steps)
            .SingleOrDefaultAsync(
                scenario => scenario.ScenarioKey == scenarioKey && scenario.IsCanonical,
                cancellationToken);
    }
}
