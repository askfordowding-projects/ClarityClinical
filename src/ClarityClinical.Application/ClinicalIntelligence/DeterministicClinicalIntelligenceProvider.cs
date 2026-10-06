using ClarityClinical.Domain.Assessments;
using ClarityClinical.Domain.Consultations;

namespace ClarityClinical.Application.ClinicalIntelligence;

public sealed class DeterministicClinicalIntelligenceProvider(MiguelScenarioRuleSet ruleSet)
    : IClinicalIntelligenceProvider
{
    public Task<ClinicalIntelligenceResult> EvaluateAsync(
        Consultation consultation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(consultation);
        cancellationToken.ThrowIfCancellationRequested();

        var includedFacts = consultation.Facts
            .Where(fact => fact.IncludeInReasoning)
            .ToArray();
        var current = ruleSet.Evaluate(includedFacts);

        var excludedFact = consultation.Facts
            .Where(fact => !fact.IncludeInReasoning)
            .OrderByDescending(fact => fact.OccurredAt)
            .FirstOrDefault();

        ClinicalFact? changedFact;
        bool isRemoval;
        IReadOnlyCollection<ClinicalFact> previousFacts;

        if (excludedFact is not null)
        {
            changedFact = excludedFact;
            isRemoval = true;
            previousFacts = [.. includedFacts, excludedFact];
        }
        else
        {
            changedFact = includedFacts
                .OrderByDescending(fact => fact.OccurredAt)
                .FirstOrDefault();
            isRemoval = false;
            previousFacts = changedFact is null
                ? []
                : includedFacts.Where(fact => fact.Id != changedFact.Id).ToArray();
        }

        var previous = ruleSet.Evaluate(previousFacts)
            .ToDictionary(candidate => candidate.Key, StringComparer.Ordinal);
        var changes = new List<AssessmentChange>();
        var candidates = new List<ClinicalAssessmentCandidateResult>();

        foreach (var candidate in current)
        {
            var previousScore = previous[candidate.Key].Score;
            var changed = changedFact is not null && previousScore != candidate.Score;
            var changeReason = changed
                ? isRemoval
                    ? $"{changedFact!.DisplayText} removed from clinical reasoning."
                    : $"{changedFact!.DisplayText} added to clinical reasoning."
                : null;

            candidates.Add(new ClinicalAssessmentCandidateResult(
                candidate.Key,
                candidate.DisplayName,
                candidate.Score,
                changed ? previousScore : null,
                PriorityScoreType.IllustrativePriority,
                candidate.SupportingEvidence,
                candidate.MissingEvidence,
                candidate.SuggestedChecks,
                candidate.Warnings,
                changeReason));

            if (changed && changeReason is not null)
            {
                changes.Add(new AssessmentChange(
                    candidate.Key,
                    previousScore,
                    candidate.Score,
                    changeReason));
            }
        }

        return Task.FromResult(new ClinicalIntelligenceResult(
            candidates,
            changes,
            MiguelScenarioRuleSet.Version));
    }
}
