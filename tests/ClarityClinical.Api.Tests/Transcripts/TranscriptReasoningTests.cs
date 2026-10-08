using System.Net.Http.Json;
using ClarityClinical.Api.Tests.TestSupport;

namespace ClarityClinical.Api.Tests.Transcripts;

public sealed class TranscriptReasoningTests
{
    [Fact]
    public async Task Transcript_facts_drive_and_reverse_the_Miguel_DVT_assessment()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        (await client.PostAsJsonAsync("/api/auth/demo-login", new { role = "Clinician" })).EnsureSuccessStatusCode();
        var consultationId = await context.CreateMiguelDemoConsultationAsync(client);
        (await client.PostAsync($"/api/consultations/{consultationId}/start", null)).EnsureSuccessStatusCode();

        var swelling = await AddSegmentAsync(
            client,
            consultationId,
            "Patient",
            "es-ES",
            "Tengo la pierna izquierda hinchada desde ayer.",
            "My left leg has been swollen since yesterday.");
        var dvt = FindAssessment(swelling, "dvt");
        Assert.Equal(48, dvt.CurrentScore);
        Assert.Contains(dvt.SupportingEvidence, evidence =>
            evidence.FactCode == "unilateral-calf-swelling" &&
            evidence.Source == "PatientReport");

        var travel = await AddSegmentAsync(
            client,
            consultationId,
            "Patient",
            "es-ES",
            "Volvi de Espana en un vuelo de ocho horas.",
            "I returned from Spain on an eight hour flight.");
        dvt = FindAssessment(travel, "dvt");
        Assert.Equal(67, dvt.CurrentScore);
        var travelSegmentId = travel.Transcript.Single(segment =>
            segment.OriginalText.Contains("vuelo de ocho horas", StringComparison.OrdinalIgnoreCase)).Id;

        var asymmetry = await AddSegmentAsync(
            client,
            consultationId,
            "Clinician",
            "en-GB",
            "There is visible calf asymmetry.",
            null);
        dvt = FindAssessment(asymmetry, "dvt");
        Assert.Equal(72, dvt.CurrentScore);
        Assert.Contains(dvt.SupportingEvidence, evidence =>
            evidence.FactCode == "calf-asymmetry" &&
            evidence.Source == "ClinicalObservation");

        var exclude = await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/transcript-segments/{travelSegmentId}/exclude",
            new { reason = "Transcription context corrected" });
        exclude.EnsureSuccessStatusCode();
        var afterExclusion = await exclude.Content.ReadFromJsonAsync<WorkspaceResponse>();
        dvt = FindAssessment(afterExclusion!, "dvt");

        Assert.Equal(53, dvt.CurrentScore);
        Assert.Equal(72, dvt.PreviousScore);
        Assert.Contains("Recent prolonged travel removed", dvt.ChangeReason, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(dvt.SupportingEvidence, evidence => evidence.FactCode == "recent-prolonged-travel");
    }

    [Fact]
    public async Task Interpreter_transcript_does_not_automatically_change_clinical_assessment()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        (await client.PostAsJsonAsync("/api/auth/demo-login", new { role = "Clinician" })).EnsureSuccessStatusCode();
        var consultationId = await context.CreateMiguelDemoConsultationAsync(client);
        (await client.PostAsync($"/api/consultations/{consultationId}/start", null)).EnsureSuccessStatusCode();

        var workspace = await AddSegmentAsync(
            client,
            consultationId,
            "Interpreter",
            "en-GB",
            "The patient says his left leg is swollen.",
            null);

        Assert.Equal(29, FindAssessment(workspace, "dvt").CurrentScore);
    }

    private static async Task<WorkspaceResponse> AddSegmentAsync(
        HttpClient client,
        Guid consultationId,
        string speakerRole,
        string sourceLanguage,
        string originalText,
        string? translatedText)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/transcript-segments",
            new
            {
                speakerRole,
                sourceLanguage,
                originalText,
                translatedText,
                recognitionConfidence = 0.95
            });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<WorkspaceResponse>())!;
    }

    private static AssessmentResponse FindAssessment(WorkspaceResponse workspace, string key) =>
        workspace.Assessments.Single(assessment => assessment.Key == key);

    private sealed record WorkspaceResponse(
        IReadOnlyList<TranscriptResponse> Transcript,
        IReadOnlyList<AssessmentResponse> Assessments);

    private sealed record TranscriptResponse(Guid Id, string OriginalText);

    private sealed record AssessmentResponse(
        string Key,
        int CurrentScore,
        int? PreviousScore,
        string? ChangeReason,
        IReadOnlyList<EvidenceResponse> SupportingEvidence);

    private sealed record EvidenceResponse(string FactCode, string Source);
}
