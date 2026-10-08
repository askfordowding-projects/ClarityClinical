using System.Net.Http.Json;
using ClarityClinical.Api.Tests.TestSupport;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClarityClinical.Api.Tests.Transcripts;

public sealed class TranscriptProjectionLifecycleTests
{
    [Fact]
    public async Task Correcting_transcript_withdraws_fact_and_reverses_score()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        var consultationId = await StartConsultationAsync(context, client);
        var added = await AddPatientSwellingAsync(client, consultationId);
        var segmentId = Assert.Single(added.Transcript).Id;
        Assert.Equal(48, Dvt(added).CurrentScore);

        var response = await client.PatchAsJsonAsync(
            $"/api/consultations/{consultationId}/transcript-segments/{segmentId}/correction",
            new { correctedText = "I only feel generally tired.", translatedText = (string?)null });
        response.EnsureSuccessStatusCode();
        var workspace = (await response.Content.ReadFromJsonAsync<WorkspaceResponse>())!;

        Assert.Equal(29, Dvt(workspace).CurrentScore);
        Assert.Equal(48, Dvt(workspace).PreviousScore);
        Assert.Contains("Unilateral calf swelling removed", Dvt(workspace).ChangeReason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Changing_patient_speech_to_interpreter_withdraws_fact_and_changing_back_restores_it()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        var consultationId = await StartConsultationAsync(context, client);
        var added = await AddPatientSwellingAsync(client, consultationId);
        var segmentId = Assert.Single(added.Transcript).Id;

        var toInterpreter = await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/transcript-segments/{segmentId}/speaker",
            new { speakerRole = "Interpreter" });
        toInterpreter.EnsureSuccessStatusCode();
        var withdrawn = (await toInterpreter.Content.ReadFromJsonAsync<WorkspaceResponse>())!;
        Assert.Equal(29, Dvt(withdrawn).CurrentScore);

        var backToPatient = await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/transcript-segments/{segmentId}/speaker",
            new { speakerRole = "Patient" });
        backToPatient.EnsureSuccessStatusCode();
        var restored = (await backToPatient.Content.ReadFromJsonAsync<WorkspaceResponse>())!;
        Assert.Equal(48, Dvt(restored).CurrentScore);
    }

    [Fact]
    public async Task Redacting_transcript_withdraws_fact_but_audit_keeps_non_text_provenance()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        var consultationId = await StartConsultationAsync(context, client);
        var added = await AddPatientSwellingAsync(client, consultationId);
        var segmentId = Assert.Single(added.Transcript).Id;

        var response = await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/transcript-segments/{segmentId}/redact",
            new { reason = "Patient requested exclusion" });
        response.EnsureSuccessStatusCode();
        var workspace = (await response.Content.ReadFromJsonAsync<WorkspaceResponse>())!;
        Assert.Equal(29, Dvt(workspace).CurrentScore);

        await using var scope = context.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ClarityClinicalDbContext>();
        var factEvents = await db.AuditEvents
            .Where(item => item.ConsultationId == consultationId && item.EntityType == "ClinicalFact")
            .OrderBy(item => item.OccurredAt)
            .ToArrayAsync();

        Assert.Contains(factEvents, item => item.Action == "ClinicalFactIdentified");
        Assert.Contains(factEvents, item => item.Action == "ClinicalFactWithdrawn");
        Assert.All(factEvents, item => Assert.Equal(segmentId.ToString("N"), item.CorrelationId));
        var metadata = string.Join(" | ", factEvents.Select(item =>
            $"{item.PreviousState} {item.NewState} {item.Reason}"));
        Assert.DoesNotContain("left leg", metadata, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("pierna", metadata, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<Guid> StartConsultationAsync(ApiTestContext context, HttpClient client)
    {
        (await client.PostAsJsonAsync("/api/auth/demo-login", new { role = "Clinician" })).EnsureSuccessStatusCode();
        var consultationId = await context.CreateMiguelDemoConsultationAsync(client);
        (await client.PostAsync($"/api/consultations/{consultationId}/start", null)).EnsureSuccessStatusCode();
        return consultationId;
    }

    private static async Task<WorkspaceResponse> AddPatientSwellingAsync(HttpClient client, Guid consultationId)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/transcript-segments",
            new
            {
                speakerRole = "Patient",
                sourceLanguage = "es-ES",
                originalText = "Tengo la pierna izquierda hinchada desde ayer.",
                translatedText = "My left leg has been swollen since yesterday.",
                recognitionConfidence = 0.95
            });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<WorkspaceResponse>())!;
    }

    private static AssessmentResponse Dvt(WorkspaceResponse workspace) =>
        workspace.Assessments.Single(assessment => assessment.Key == "dvt");

    private sealed record WorkspaceResponse(
        IReadOnlyList<TranscriptResponse> Transcript,
        IReadOnlyList<AssessmentResponse> Assessments);

    private sealed record TranscriptResponse(Guid Id);

    private sealed record AssessmentResponse(
        string Key,
        int CurrentScore,
        int? PreviousScore,
        string? ChangeReason);
}
