using ClarityClinical.Domain.Transcripts;

namespace ClarityClinical.Application.Transcripts;

public interface ITranscriptSegmentRepository
{
    Task AddAsync(TranscriptSegment segment, CancellationToken cancellationToken);

    Task<IReadOnlyList<TranscriptSegment>> GetByConsultationAsync(
        Guid consultationId,
        CancellationToken cancellationToken);
}
