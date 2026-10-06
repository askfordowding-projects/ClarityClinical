using System.Globalization;
using ClarityClinical.Application.Audit;
using ClarityClinical.Application.ClinicalIntelligence;
using ClarityClinical.Domain.Consultations;

namespace ClarityClinical.Application.Consultations.Workspace;

public sealed class AddClinicalFactService(
    IConsultationRepository consultationRepository,
    IClinicalIntelligenceProvider clinicalIntelligenceProvider,
    IAuditWriter auditWriter)
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

        var fact = new ClinicalFact(
            Guid.NewGuid(),
            code,
            displayText,
            source,
            DateTimeOffset.UtcNow);
        consultation.AddFact(fact);
        await consultationRepository.SaveChangesAsync(cancellationToken);

        var result = await clinicalIntelligenceProvider.EvaluateAsync(consultation, cancellationToken);
        var correlationId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
        await auditWriter.WriteAsync(new AuditWriteRequest(
            consultationId,
            "ClinicalFactAdded",
            nameof(ClinicalFact),
            fact.Id.ToString(),
            correlationId,
            NewState: $"{fact.Code}:{fact.Source}"), cancellationToken);
        await auditWriter.WriteAsync(new AuditWriteRequest(
            consultationId,
            "RuleEvaluated",
            "ClinicalRuleSet",
            result.RuleSetVersion,
            correlationId,
            RuleVersion: result.RuleSetVersion), cancellationToken);

        foreach (var change in result.Changes)
        {
            await auditWriter.WriteAsync(new AuditWriteRequest(
                consultationId,
                "AssessmentChanged",
                "AssessmentCandidate",
                change.CandidateKey,
                correlationId,
                PreviousState: change.PreviousScore.ToString(CultureInfo.InvariantCulture),
                NewState: change.CurrentScore.ToString(CultureInfo.InvariantCulture),
                Reason: change.Reason,
                RuleVersion: result.RuleSetVersion), cancellationToken);
        }

        return result;
    }
}
