using ClarityClinical.Domain.Consultations;

namespace ClarityClinical.Application.ClinicalIntelligence;

public sealed class MiguelScenarioRuleSet
{
    public const string Version = "demo-illustrative-1.0";

    internal IReadOnlyList<RuleCandidateEvaluation> Evaluate(
        IReadOnlyCollection<ClinicalFact> facts)
    {
        var activeFacts = facts
            .GroupBy(fact => fact.Code, StringComparer.Ordinal)
            .Select(group => group.OrderByDescending(fact => fact.OccurredAt).First())
            .ToArray();

        return
        [
            EvaluateCandidate(
                "dvt",
                "Deep vein thrombosis",
                29,
                new Dictionary<string, int>(StringComparer.Ordinal)
                {
                    ["unilateral-calf-swelling"] = 19,
                    ["recent-prolonged-travel"] = 19,
                    ["calf-asymmetry"] = 5
                },
                activeFacts,
                [("wells-assessment", "Wells assessment"), ("d-dimer", "D-dimer")],
                ["Complete Wells assessment", "Follow the local DVT pathway"],
                ["Escalate immediately if chest pain or breathlessness develops"]),
            EvaluateCandidate(
                "cellulitis",
                "Cellulitis",
                20,
                new Dictionary<string, int>(StringComparer.Ordinal)
                {
                    ["ankle-wound-warmth"] = 28,
                    ["type-2-diabetes"] = 10
                },
                activeFacts,
                [("wound-assessment", "Wound assessment"), ("systemic-signs", "Systemic signs")],
                ["Examine the wound and surrounding skin", "Check for systemic signs"],
                []),
            EvaluateCandidate(
                "musculoskeletal",
                "Musculoskeletal inflammation",
                21,
                new Dictionary<string, int>(StringComparer.Ordinal)
                {
                    ["knee-pain"] = 10,
                    ["restricted-mobility"] = 10
                },
                activeFacts,
                [("injury-history", "Injury history and onset")],
                ["Assess the joint, injury history and symptom onset"],
                [])
        ];
    }

    private static RuleCandidateEvaluation EvaluateCandidate(
        string key,
        string displayName,
        int baseline,
        IReadOnlyDictionary<string, int> weights,
        IReadOnlyCollection<ClinicalFact> activeFacts,
        IReadOnlyCollection<(string Code, string Label)> requiredEvidence,
        IReadOnlyList<string> suggestedChecks,
        IReadOnlyList<string> warnings)
    {
        var supportingFacts = activeFacts
            .Where(fact => weights.ContainsKey(fact.Code))
            .OrderBy(fact => fact.OccurredAt)
            .ToArray();
        var score = Math.Clamp(
            baseline + supportingFacts.Sum(fact => weights[fact.Code]),
            0,
            100);
        var activeCodes = activeFacts
            .Select(fact => fact.Code)
            .ToHashSet(StringComparer.Ordinal);
        var missingEvidence = requiredEvidence
            .Where(required => !activeCodes.Contains(required.Code))
            .Select(required => required.Label)
            .ToArray();

        return new RuleCandidateEvaluation(
            key,
            displayName,
            score,
            supportingFacts
                .Select(fact => new ClinicalEvidenceResult(fact.Id, fact.Code, fact.DisplayText, fact.Source))
                .ToArray(),
            missingEvidence,
            suggestedChecks,
            warnings);
    }
}

internal sealed record RuleCandidateEvaluation(
    string Key,
    string DisplayName,
    int Score,
    IReadOnlyList<ClinicalEvidenceResult> SupportingEvidence,
    IReadOnlyList<string> MissingEvidence,
    IReadOnlyList<string> SuggestedChecks,
    IReadOnlyList<string> Warnings);
