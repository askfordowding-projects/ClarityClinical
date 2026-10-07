using ClarityClinical.Application.Transcripts;
using ClarityClinical.Domain.Transcripts;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClarityClinical.Infrastructure.Transcripts;

public sealed class TranscriptSegmentRepository(
    ClarityClinicalDbContext dbContext) : ITranscriptSegmentRepository
{
    public async Task AddAsync(
        TranscriptSegment segment,
        CancellationToken cancellationToken)
    {
        dbContext.TranscriptSegments.Add(segment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TranscriptSegment>> GetByConsultationAsync(
        Guid consultationId,
        CancellationToken cancellationToken) =>
        await dbContext.TranscriptSegments
            .AsNoTracking()
            .Where(segment => segment.ConsultationId == consultationId)
            .OrderBy(segment => segment.OccurredAt)
            .ToListAsync(cancellationToken);
}
