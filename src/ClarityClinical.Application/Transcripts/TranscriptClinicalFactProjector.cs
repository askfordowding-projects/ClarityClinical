using ClarityClinical.Application.Audit;
using ClarityClinical.Application.Consultations;
using ClarityClinical.Domain.Consultations;
using ClarityClinical.Domain.Transcripts;

namespace ClarityClinical.Application.Transcripts;

public sealed class TranscriptClinicalFactProjector(
    IConsultationRepository consultationRepository,
    TranscriptClinicalFactExtractor extractor,
    IAuditWriter auditWriter)
{
    public async Task ProjectAsync(
        TranscriptSegment segment,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(segment);

        var consultation = await consultationRepository.GetAsync(
            segment.ConsultationId,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Consultation '{segment.ConsultationId}' was not found.");

        var previous = consultation.Facts
            .Where(fact => fact.SourceEventId == segment.Id)
            .ToDictionary(
                FactKey,
                fact => new FactProjectionSnapshot(fact.Id, fact.IncludeInReasoning),
                StringComparer.Ordinal);
        var desired = extractor.Extract(segment);

        consultation.SynchronizeFactsFromEvent(segment.Id, desired);
        await consultationRepository.SaveChangesAsync(cancellationToken);

        var current = consultation.Facts
            .Where(fact => fact.SourceEventId == segment.Id)
            .ToDictionary(FactKey, StringComparer.Ordinal);
        var correlationId = segment.Id.ToString("N");

        foreach (var (key, fact) in current)
        {
            if (!previous.TryGetValue(key, out var oldState) && fact.IncludeInReasoning)
            {
                await WriteFactAuditAsync(
                    segment,
                    fact,
                    "ClinicalFactIdentified",
                    correlationId,
                    newState: $"{fact.Code}:{fact.Source}:Included",
                    reason: "Derived from transcript segment.",
                    cancellationToken: cancellationToken);
            }
            else if (oldState is not null && !oldState.IncludeInReasoning && fact.IncludeInReasoning)
            {
                await WriteFactAuditAsync(
                    segment,
                    fact,
                    "ClinicalFactRestored",
                    correlationId,
                    previousState: $"{fact.Code}:{fact.Source}:Excluded",
                    newState: $"{fact.Code}:{fact.Source}:Included",
                    reason: "Transcript segment again supports this fact.",
                    cancellationToken: cancellationToken);
            }
            else if (oldState is not null && oldState.IncludeInReasoning && !fact.IncludeInReasoning)
            {
                await WriteFactAuditAsync(
                    segment,
                    fact,
                    "ClinicalFactWithdrawn",
                    correlationId,
                    previousState: $"{fact.Code}:{fact.Source}:Included",
                    newState: $"{fact.Code}:{fact.Source}:Excluded",
                    reason: "Transcript segment no longer supports this fact.",
                    cancellationToken: cancellationToken);
            }
        }
    }

    private Task WriteFactAuditAsync(
        TranscriptSegment segment,
        ClinicalFact fact,
        string action,
        string correlationId,
        string? previousState = null,
        string? newState = null,
        string? reason = null,
        CancellationToken cancellationToken = default) =>
        auditWriter.WriteAsync(new AuditWriteRequest(
            segment.ConsultationId,
            action,
            nameof(ClinicalFact),
            fact.Id.ToString(),
            correlationId,
            PreviousState: previousState,
            NewState: newState,
            Reason: reason), cancellationToken);

    private static string FactKey(ClinicalFact fact) =>
        $"{fact.Code}|{fact.Source}";

    private sealed record FactProjectionSnapshot(Guid Id, bool IncludeInReasoning);
}

