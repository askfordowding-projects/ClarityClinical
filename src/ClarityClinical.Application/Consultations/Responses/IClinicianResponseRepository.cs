using ClarityClinical.Domain.Assessments;

namespace ClarityClinical.Application.Consultations.Responses;

public interface IClinicianResponseRepository
{
    Task AddAsync(
        ClinicianResponse response,
        CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<string, ClinicianResponse>> GetLatestByConsultationAsync(
        Guid consultationId,
        CancellationToken cancellationToken);
}
