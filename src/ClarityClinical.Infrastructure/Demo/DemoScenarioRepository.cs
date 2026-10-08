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

    public async Task<DemoScenario?> GetScenarioForVisitorAsync(
        string scenarioKey,
        string visitorId,
        CancellationToken cancellationToken)
    {
        var canonical = await dbContext.DemoScenarios
            .AsNoTracking()
            .Where(x => x.ScenarioKey == scenarioKey && x.IsCanonical)
            .Select(x => new { x.Id })
            .SingleOrDefaultAsync(cancellationToken);
        if (canonical is null) return null;

        var sandboxScenarioId = await dbContext.DemoSandboxes
            .Where(x => x.VisitorId == visitorId && x.SourceScenarioId == canonical.Id)
            .Select(x => (Guid?)x.SandboxScenarioId)
            .SingleOrDefaultAsync(cancellationToken);
        var scenarioId = sandboxScenarioId ?? canonical.Id;
        return await dbContext.DemoScenarios
            .Include(x => x.Steps)
            .SingleOrDefaultAsync(x => x.Id == scenarioId, cancellationToken);
    }
}
