using ClarityClinical.Domain.Patients;

namespace ClarityClinical.Application.Patients;

public interface IPatientRepository
{
    Task<Patient?> GetAsync(Guid patientId, CancellationToken cancellationToken);
}
