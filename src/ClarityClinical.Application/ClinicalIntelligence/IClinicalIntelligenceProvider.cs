using ClarityClinical.Domain.Consultations;

namespace ClarityClinical.Application.ClinicalIntelligence;

public interface IClinicalIntelligenceProvider
{
    Task<ClinicalIntelligenceResult> EvaluateAsync(
        Consultation consultation,
        CancellationToken cancellationToken);
}
