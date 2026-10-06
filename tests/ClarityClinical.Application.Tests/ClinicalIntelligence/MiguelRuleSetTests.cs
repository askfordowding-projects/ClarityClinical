using ClarityClinical.Application.ClinicalIntelligence;
using ClarityClinical.Domain.Consultations;

namespace ClarityClinical.Application.Tests.ClinicalIntelligence;

public sealed class MiguelRuleSetTests
{
    private static readonly DateTimeOffset BaseTime =
        new(2026, 10, 6, 14, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Unilateral_calf_swelling_sets_DVT_priority_to_48()
    {
        var consultation = CreateConsultation();
        AddFact(consultation, "unilateral-calf-swelling", "Unilateral calf swelling", ClinicalFactSource.PatientReport, 1);

        var result = await CreateProvider().EvaluateAsync(consultation, CancellationToken.None);

        Assert.Equal(48, Candidate(result, "dvt").CurrentScore);
    }

    [Fact]
    public async Task Prolonged_travel_raises_DVT_priority_to_67()
    {
        var consultation = CreateConsultation();
        AddFact(consultation, "unilateral-calf-swelling", "Unilateral calf swelling", ClinicalFactSource.PatientReport, 1);
        AddFact(consultation, "recent-prolonged-travel", "Recent prolonged travel", ClinicalFactSource.PatientReport, 2);

        var result = await CreateProvider().EvaluateAsync(consultation, CancellationToken.None);

        Assert.Equal(67, Candidate(result, "dvt").CurrentScore);
        Assert.Equal(48, Candidate(result, "dvt").PreviousScore);
    }

    [Fact]
    public async Task Calf_asymmetry_raises_DVT_priority_to_72()
    {
        var consultation = CreateConsultation();
        AddFact(consultation, "unilateral-calf-swelling", "Unilateral calf swelling", ClinicalFactSource.PatientReport, 1);
        AddFact(consultation, "recent-prolonged-travel", "Recent prolonged travel", ClinicalFactSource.PatientReport, 2);
        AddFact(consultation, "calf-asymmetry", "Unilateral calf asymmetry", ClinicalFactSource.ClinicalObservation, 3);

        var result = await CreateProvider().EvaluateAsync(consultation, CancellationToken.None);

        Assert.Equal(72, Candidate(result, "dvt").CurrentScore);
        Assert.Equal(67, Candidate(result, "dvt").PreviousScore);
    }

    [Fact]
    public async Task Ankle_wound_warmth_and_diabetes_set_cellulitis_priority_to_58()
    {
        var consultation = CreateConsultation();
        AddFact(consultation, "type-2-diabetes", "Type 2 diabetes", ClinicalFactSource.EstablishedRecord, 1);
        AddFact(consultation, "ankle-wound-warmth", "Ankle wound with local warmth", ClinicalFactSource.ClinicalObservation, 2);

        var result = await CreateProvider().EvaluateAsync(consultation, CancellationToken.None);

        Assert.Equal(58, Candidate(result, "cellulitis").CurrentScore);
    }

    [Fact]
    public async Task Knee_pain_and_restricted_mobility_set_musculoskeletal_priority_to_41()
    {
        var consultation = CreateConsultation();
        AddFact(consultation, "knee-pain", "Knee pain", ClinicalFactSource.PatientReport, 1);
        AddFact(consultation, "restricted-mobility", "Restricted mobility", ClinicalFactSource.ClinicalObservation, 2);

        var result = await CreateProvider().EvaluateAsync(consultation, CancellationToken.None);

        Assert.Equal(41, Candidate(result, "musculoskeletal").CurrentScore);
    }

    [Fact]
    public async Task Excluding_prolonged_travel_removes_its_DVT_contribution_and_explains_change()
    {
        var consultation = CreateConsultation();
        AddFact(consultation, "unilateral-calf-swelling", "Unilateral calf swelling", ClinicalFactSource.PatientReport, 1);
        var travel = AddFact(
            consultation,
            "recent-prolonged-travel",
            "Recent prolonged travel",
            ClinicalFactSource.PatientReport,
            2);

        var before = await CreateProvider().EvaluateAsync(consultation, CancellationToken.None);
        Assert.Equal(67, Candidate(before, "dvt").CurrentScore);

        travel.ExcludeFromReasoning();
        var after = await CreateProvider().EvaluateAsync(consultation, CancellationToken.None);
        var dvt = Candidate(after, "dvt");

        Assert.Equal(48, dvt.CurrentScore);
        Assert.Equal(67, dvt.PreviousScore);
        Assert.Contains("removed", dvt.ChangeReason, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("prolonged travel", dvt.ChangeReason, StringComparison.OrdinalIgnoreCase);
    }

    private static Consultation CreateConsultation()
    {
        var consultation = new Consultation(Guid.NewGuid(), Guid.NewGuid());
        consultation.Start(BaseTime);
        return consultation;
    }

    private static ClinicalFact AddFact(
        Consultation consultation,
        string code,
        string displayText,
        ClinicalFactSource source,
        int minute)
    {
        var fact = new ClinicalFact(Guid.NewGuid(), code, displayText, source, BaseTime.AddMinutes(minute));
        consultation.AddFact(fact);
        return fact;
    }

    private static DeterministicClinicalIntelligenceProvider CreateProvider() =>
        new(new MiguelScenarioRuleSet());

    private static ClinicalAssessmentCandidateResult Candidate(
        ClinicalIntelligenceResult result,
        string key) =>
        result.Candidates.Single(candidate => candidate.Key == key);
}
