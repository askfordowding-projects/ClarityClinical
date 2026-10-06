using ClarityClinical.Domain.Consultations;

namespace ClarityClinical.Application.Consultations;

public sealed class ConsultationLifecycleService(IConsultationRepository repository)
{
    public async Task<Consultation> StartAsync(
        Guid consultationId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var consultation = await GetRequiredAsync(consultationId, cancellationToken);
        consultation.Start(now);
        await repository.SaveChangesAsync(cancellationToken);
        return consultation;
    }

    public async Task<Consultation> CompleteAsync(
        Guid consultationId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var consultation = await GetRequiredAsync(consultationId, cancellationToken);
        consultation.Complete(now);
        await repository.SaveChangesAsync(cancellationToken);
        return consultation;
    }

    private async Task<Consultation> GetRequiredAsync(
        Guid consultationId,
        CancellationToken cancellationToken)
    {
        return await repository.GetAsync(consultationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Consultation '{consultationId}' was not found.");
    }
}
