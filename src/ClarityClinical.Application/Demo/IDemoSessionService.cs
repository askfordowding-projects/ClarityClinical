using ClarityClinical.Domain.Demo;

namespace ClarityClinical.Application.Demo;

public interface IDemoSessionService
{
    Task<DemoSession> CreateAsync(
        string scenarioKey,
        string visitorId,
        CancellationToken cancellationToken);

    Task<DemoSession?> GetAsync(
        Guid sessionId,
        string visitorId,
        CancellationToken cancellationToken);

    Task<DemoSession> ResetAsync(
        Guid sessionId,
        string visitorId,
        CancellationToken cancellationToken);

    Task<bool> OwnsConsultationAsync(
        Guid consultationId,
        string visitorId,
        CancellationToken cancellationToken);

    Task<int> DeleteExpiredAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken);
}
