using ClarityClinical.Application.Patients;
using ClarityClinical.Domain.Patients;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClarityClinical.Infrastructure.Patients;

public sealed class PatientRepository(ClarityClinicalDbContext dbContext) : IPatientRepository
{
    public Task<Patient?> GetAsync(Guid patientId, CancellationToken cancellationToken)
    {
        return dbContext.Patients.SingleOrDefaultAsync(
            patient => patient.Id == patientId,
            cancellationToken);
    }
}
