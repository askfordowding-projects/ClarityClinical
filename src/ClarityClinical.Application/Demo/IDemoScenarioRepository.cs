using ClarityClinical.Domain.Demo;

namespace ClarityClinical.Application.Demo;

public interface IDemoScenarioRepository
{
    Task<DemoScenario?> GetCanonicalScenarioAsync(
        string scenarioKey,
        CancellationToken cancellationToken);

    Task<DemoScenario?> GetScenarioForVisitorAsync(
        string scenarioKey,
        string visitorId,
        CancellationToken cancellationToken);
}
