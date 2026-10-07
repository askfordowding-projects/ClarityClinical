using ClarityClinical.Application.Consultations.Responses;
using ClarityClinical.Domain.Assessments;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClarityClinical.Infrastructure.Consultations.Responses;

public sealed class ClinicianResponseRepository(ClarityClinicalDbContext dbContext)
    : IClinicianResponseRepository
{
    public async Task AddAsync(
        ClinicianResponse response,
        CancellationToken cancellationToken)
    {
        dbContext.ClinicianResponses.Add(response);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, ClinicianResponse>> GetLatestByConsultationAsync(
        Guid consultationId,
        CancellationToken cancellationToken)
    {
        var responses = await dbContext.ClinicianResponses
            .AsNoTracking()
            .Where(response => response.ConsultationId == consultationId)
            .OrderByDescending(response => response.RespondedAt)
            .ToListAsync(cancellationToken);

        return responses
            .GroupBy(response => response.RecommendationKey, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
    }
}
