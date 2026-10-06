using ClarityClinical.Application.ClinicalIntelligence;
using ClarityClinical.Domain.Consultations;

namespace ClarityClinical.Application.Consultations.Workspace;

public sealed class AddClinicalFactService(
    IConsultationRepository consultationRepository,
    IClinicalIntelligenceProvider clinicalIntelligenceProvider)
{
    public async Task<ClinicalIntelligenceResult> AddAsync(
        Guid consultationId,
        string code,
        string displayText,
        ClinicalFactSource source,
        CancellationToken cancellationToken)
    {
        var consultation = await consultationRepository.GetAsync(consultationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Consultation '{consultationId}' was not found.");

        if (consultation.Status != ConsultationStatus.InProgress)
        {
            throw new InvalidOperationException("Clinical facts can only be added to an in progress consultation.");
        }

        consultation.AddFact(new ClinicalFact(
            Guid.NewGuid(),
            code,
            displayText,
            source,
            DateTimeOffset.UtcNow));
        await consultationRepository.SaveChangesAsync(cancellationToken);

        return await clinicalIntelligenceProvider.EvaluateAsync(consultation, cancellationToken);
    }
}
