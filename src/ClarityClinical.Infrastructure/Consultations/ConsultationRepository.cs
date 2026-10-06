using ClarityClinical.Application.Consultations;
using ClarityClinical.Domain.Consultations;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClarityClinical.Infrastructure.Consultations;

public sealed class ConsultationRepository(ClarityClinicalDbContext dbContext) : IConsultationRepository
{
    public Task<Consultation?> GetAsync(Guid consultationId, CancellationToken cancellationToken)
    {
        return dbContext.Consultations
            .Include(consultation => consultation.Facts)
            .SingleOrDefaultAsync(
                consultation => consultation.Id == consultationId,
                cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
