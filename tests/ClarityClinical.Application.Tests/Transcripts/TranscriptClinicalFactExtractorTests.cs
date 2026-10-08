using ClarityClinical.Application.Transcripts;
using ClarityClinical.Domain.Consultations;
using ClarityClinical.Domain.Transcripts;

namespace ClarityClinical.Application.Tests.Transcripts;

public sealed class TranscriptClinicalFactExtractorTests
{
    private readonly TranscriptClinicalFactExtractor _extractor = new();

    [Fact]
    public void Patient_leg_swelling_extracts_patient_report_with_transcript_provenance()
    {
        var segment = Segment(
            ConsultationSpeakerRole.Patient,
            "Tengo la pierna izquierda hinchada desde ayer.",
            "My left leg has been swollen since yesterday.");

        var facts = _extractor.Extract(segment);

        var fact = Assert.Single(facts);
        Assert.Equal("unilateral-calf-swelling", fact.Code);
        Assert.Equal("Unilateral calf swelling", fact.DisplayText);
        Assert.Equal(ClinicalFactSource.PatientReport, fact.Source);
        Assert.Equal(segment.Id, fact.SourceEventId);
    }

    [Fact]
    public void Patient_prolonged_flight_extracts_travel_risk_factor()
    {
        var segment = Segment(
            ConsultationSpeakerRole.Patient,
            "Volvi de Espana en un vuelo de ocho horas.",
            "I returned from Spain on an eight hour flight.");

        var fact = Assert.Single(_extractor.Extract(segment));

        Assert.Equal("recent-prolonged-travel", fact.Code);
        Assert.Equal(ClinicalFactSource.PatientReport, fact.Source);
    }

    [Fact]
    public void Clinician_calf_asymmetry_is_a_clinical_observation()
    {
        var segment = Segment(
            ConsultationSpeakerRole.Clinician,
            "There is visible calf asymmetry.",
            null,
            "en-GB");

        var fact = Assert.Single(_extractor.Extract(segment));

        Assert.Equal("calf-asymmetry", fact.Code);
        Assert.Equal(ClinicalFactSource.ClinicalObservation, fact.Source);
    }

    [Theory]
    [InlineData(ConsultationSpeakerRole.Carer)]
    [InlineData(ConsultationSpeakerRole.FamilyMember)]
    [InlineData(ConsultationSpeakerRole.Interpreter)]
    [InlineData(ConsultationSpeakerRole.Other)]
    [InlineData(ConsultationSpeakerRole.Unknown)]
    public void Unsupported_speaker_roles_do_not_automatically_create_clinical_facts(
        ConsultationSpeakerRole role)
    {
        var segment = Segment(role, "My left leg is swollen.", null, "en-GB");

        Assert.Empty(_extractor.Extract(segment));
    }

    [Fact]
    public void Excluded_or_redacted_transcript_does_not_create_facts()
    {
        var excluded = Segment(ConsultationSpeakerRole.Patient, "My left leg is swollen.", null, "en-GB");
        excluded.ExcludeFromReasoning();
        var redacted = Segment(ConsultationSpeakerRole.Patient, "My left leg is swollen.", null, "en-GB");
        redacted.Redact();

        Assert.Empty(_extractor.Extract(excluded));
        Assert.Empty(_extractor.Extract(redacted));
    }

    [Fact]
    public void Corrected_text_is_used_for_extraction()
    {
        var segment = Segment(ConsultationSpeakerRole.Patient, "My leg hurts.", null, "en-GB");
        segment.Correct("My left calf is swollen.", null);

        var fact = Assert.Single(_extractor.Extract(segment));

        Assert.Equal("unilateral-calf-swelling", fact.Code);
    }

    private static TranscriptSegment Segment(
        ConsultationSpeakerRole role,
        string original,
        string? translated,
        string language = "es-ES") =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            role,
            language,
            original,
            translated,
            0.93,
            DateTimeOffset.UtcNow);
}
