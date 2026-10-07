using System.Globalization;
using ClarityClinical.Application.Audit;
using ClarityClinical.Application.ClinicalIntelligence;
using ClarityClinical.Domain.Consultations;

namespace ClarityClinical.Application.Consultations.Workspace;

public sealed class ExcludeClinicalFactService(
    IConsultationRepository consultationRepository,
    IClinicalIntelligenceProvider clinicalIntelligenceProvider,
    IAuditWriter auditWriter)
{
    public async Task<ClinicalIntelligenceResult> ExcludeAsync(
        Guid consultationId,
        Guid factId,
        CancellationToken cancellationToken)
    {
        var consultation = await consultationRepository.GetAsync(consultationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Consultation '{consultationId}' was not found.");

        if (consultation.Status != ConsultationStatus.InProgress)
        {
            throw new InvalidOperationException("Clinical facts can only be excluded during an in progress consultation.");
        }

        var fact = consultation.Facts.SingleOrDefault(item => item.Id == factId)
            ?? throw new KeyNotFoundException($"Clinical fact '{factId}' was not found.");

        if (!fact.IncludeInReasoning)
        {
            throw new InvalidOperationException("The clinical fact is already excluded from reasoning.");
        }

        fact.ExcludeFromReasoning();
        await consultationRepository.SaveChangesAsync(cancellationToken);

        var result = await clinicalIntelligenceProvider.EvaluateAsync(consultation, cancellationToken);
        var correlationId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);

        await auditWriter.WriteAsync(new AuditWriteRequest(
            consultationId,
            "ClinicalFactExcluded",
            nameof(ClinicalFact),
            fact.Id.ToString(),
            correlationId,
            PreviousState: "IncludedInReasoning",
            NewState: "ExcludedFromReasoning",
            Reason: $"{fact.DisplayText} removed from clinical reasoning."), cancellationToken);

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
