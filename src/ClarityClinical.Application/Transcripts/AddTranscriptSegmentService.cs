using ClarityClinical.Application.Audit;
using ClarityClinical.Application.Consultations;
using ClarityClinical.Domain.Consultations;
using ClarityClinical.Domain.Transcripts;

namespace ClarityClinical.Application.Transcripts;

public sealed class AddTranscriptSegmentService(
    IConsultationRepository consultationRepository,
    ITranscriptSegmentRepository transcriptRepository,
    IAuditWriter auditWriter)
{
    public async Task<TranscriptSegment> AddAsync(
        Guid consultationId,
        ConsultationSpeakerRole speakerRole,
        string sourceLanguage,
        string originalText,
        string? translatedText,
        double? recognitionConfidence,
        CancellationToken cancellationToken)
    {
        var consultation = await consultationRepository.GetAsync(consultationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Consultation '{consultationId}' was not found.");

        if (consultation.Status != ConsultationStatus.InProgress)
        {
            throw new InvalidOperationException(
                "Transcript segments can only be added to an in progress consultation.");
        }

        var segment = new TranscriptSegment(
            Guid.NewGuid(),
            consultationId,
            speakerRole,
            sourceLanguage,
            originalText,
            translatedText,
            recognitionConfidence,
            DateTimeOffset.UtcNow);

        await transcriptRepository.AddAsync(segment, cancellationToken);
        await auditWriter.WriteAsync(new AuditWriteRequest(
            consultationId,
            "TranscriptSegmentAdded",
            nameof(TranscriptSegment),
            segment.Id.ToString(),
            Guid.NewGuid().ToString("N"),
            NewState: $"{segment.SpeakerRole}:{segment.SourceLanguage}"), cancellationToken);

        return segment;
    }
}
