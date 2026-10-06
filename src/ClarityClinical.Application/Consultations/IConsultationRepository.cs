using ClarityClinical.Domain.Consultations;

namespace ClarityClinical.Application.Consultations;

public interface IConsultationRepository
{
    Task<Consultation?> GetAsync(Guid consultationId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
