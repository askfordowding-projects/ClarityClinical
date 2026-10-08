using ClarityClinical.Application.Audit;
using ClarityClinical.Application.Consultations;
using ClarityClinical.Domain.Consultations;
using ClarityClinical.Domain.Transcripts;

namespace ClarityClinical.Application.Transcripts;

public sealed class ManageTranscriptSegmentService(
    IConsultationRepository consultationRepository,
    ITranscriptSegmentRepository transcriptRepository,
    IAuditWriter auditWriter,
    TranscriptClinicalFactProjector factProjector)
{
    public async Task CorrectAsync(
        Guid consultationId,
        Guid segmentId,
        string correctedText,
        string? translatedText,
        CancellationToken cancellationToken)
    {
        var segment = await GetEditableSegmentAsync(consultationId, segmentId, cancellationToken);
        EnsureNotRedacted(segment, "corrected");

        segment.Correct(correctedText, translatedText);
        await transcriptRepository.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(
            consultationId,
            segment.Id,
            "TranscriptCorrected",
            newState: "Corrected=true",
            cancellationToken: cancellationToken);
        await factProjector.ProjectAsync(segment, cancellationToken);
    }

    public async Task ChangeSpeakerAsync(
        Guid consultationId,
        Guid segmentId,
        ConsultationSpeakerRole speakerRole,
        CancellationToken cancellationToken)
    {
        var segment = await GetEditableSegmentAsync(consultationId, segmentId, cancellationToken);
        EnsureNotRedacted(segment, "re-attributed");
        var previous = segment.SpeakerRole.ToString();

        segment.ChangeSpeaker(speakerRole);
        await transcriptRepository.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(
            consultationId,
            segment.Id,
            "TranscriptSpeakerChanged",
            previousState: previous,
            newState: segment.SpeakerRole.ToString(),
            cancellationToken: cancellationToken);
        await factProjector.ProjectAsync(segment, cancellationToken);
    }

    public async Task ExcludeFromReasoningAsync(
        Guid consultationId,
        Guid segmentId,
        string reason,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A reason is required when excluding transcript text from reasoning.", nameof(reason));
        }

        var segment = await GetEditableSegmentAsync(consultationId, segmentId, cancellationToken);
        EnsureNotRedacted(segment, "excluded from reasoning");

        segment.ExcludeFromReasoning();
        await transcriptRepository.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(
            consultationId,
            segment.Id,
            "TranscriptExcludedFromReasoning",
            previousState: "IncludeInReasoning=true",
            newState: "IncludeInReasoning=false",
            reason: reason.Trim(),
            cancellationToken: cancellationToken);
        await factProjector.ProjectAsync(segment, cancellationToken);
    }

    public async Task RedactAsync(
        Guid consultationId,
        Guid segmentId,
        string reason,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A reason is required when redacting transcript text.", nameof(reason));
        }

        var segment = await GetEditableSegmentAsync(consultationId, segmentId, cancellationToken);
        EnsureNotRedacted(segment, "redacted again");
        var previous = $"IncludeInReasoning={segment.IncludeInReasoning}";

        segment.Redact();
        await transcriptRepository.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(
            consultationId,
            segment.Id,
            "TranscriptRedacted",
            previousState: previous,
            newState: "IsRedacted=true;IncludeInReasoning=false",
            reason: reason.Trim(),
            cancellationToken: cancellationToken);
        await factProjector.ProjectAsync(segment, cancellationToken);
    }

    private async Task<TranscriptSegment> GetEditableSegmentAsync(
        Guid consultationId,
        Guid segmentId,
        CancellationToken cancellationToken)
    {
        var consultation = await consultationRepository.GetAsync(consultationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Consultation '{consultationId}' was not found.");
        if (consultation.Status != ConsultationStatus.InProgress)
        {
            throw new InvalidOperationException(
                "Transcript segments can only be changed during an in progress consultation.");
        }

        var segment = await transcriptRepository.GetAsync(segmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Transcript segment '{segmentId}' was not found.");
        if (segment.ConsultationId != consultationId)
        {
            throw new KeyNotFoundException($"Transcript segment '{segmentId}' was not found.");
        }

        return segment;
    }

    private static void EnsureNotRedacted(TranscriptSegment segment, string action)
    {
        if (segment.IsRedacted)
        {
            throw new InvalidOperationException(
                $"Redacted transcript segments cannot be {action}.");
        }
    }

    private Task WriteAuditAsync(
        Guid consultationId,
        Guid segmentId,
        string action,
        string? previousState = null,
        string? newState = null,
        string? reason = null,
        CancellationToken cancellationToken = default) =>
        auditWriter.WriteAsync(new AuditWriteRequest(
            consultationId,
            action,
            nameof(TranscriptSegment),
            segmentId.ToString(),
            segmentId.ToString("N"),
            PreviousState: previousState,
            NewState: newState,
            Reason: reason), cancellationToken);
}
