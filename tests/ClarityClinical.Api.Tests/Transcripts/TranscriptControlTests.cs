using System.Net;
using System.Net.Http.Json;
using ClarityClinical.Api.Tests.TestSupport;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClarityClinical.Api.Tests.Transcripts;

public sealed class TranscriptControlTests
{
    [Fact]
    public async Task Clinician_can_correct_a_transcript_segment()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        await LoginAsync(client);
        var consultationId = await context.CreateMiguelDemoConsultationAsync(client);
        (await client.PostAsync($"/api/consultations/{consultationId}/start", null)).EnsureSuccessStatusCode();
        var created = await AddSegmentAsync(client, consultationId);
        var segmentId = Assert.Single(created.Transcript).Id;

        var response = await client.PatchAsJsonAsync(
            $"/api/consultations/{consultationId}/transcript-segments/{segmentId}/correction",
            new { correctedText = "Tengo la pierna izquierda hinchada desde ayer.", translatedText = "My left leg has been swollen since yesterday." });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var workspace = await response.Content.ReadFromJsonAsync<WorkspaceResponse>();
        var segment = Assert.Single(workspace!.Transcript);
        Assert.True(segment.Corrected);
        Assert.Equal("Tengo la pierna izquierda hinchada desde ayer.", segment.OriginalText);
    }

    [Fact]
    public async Task Clinician_can_change_transcript_speaker()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        await LoginAsync(client);
        var consultationId = await context.CreateMiguelDemoConsultationAsync(client);
        (await client.PostAsync($"/api/consultations/{consultationId}/start", null)).EnsureSuccessStatusCode();
        var created = await AddSegmentAsync(client, consultationId);
        var segmentId = Assert.Single(created.Transcript).Id;

        var response = await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/transcript-segments/{segmentId}/speaker",
            new { speakerRole = "Interpreter" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var workspace = await response.Content.ReadFromJsonAsync<WorkspaceResponse>();
        Assert.Equal("Interpreter", Assert.Single(workspace!.Transcript).SpeakerRole);
    }
    [Fact]
    public async Task Clinician_can_exclude_transcript_from_reasoning_without_hiding_it()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        await LoginAsync(client);
        var consultationId = await context.CreateMiguelDemoConsultationAsync(client);
        (await client.PostAsync($"/api/consultations/{consultationId}/start", null)).EnsureSuccessStatusCode();
        var created = await AddSegmentAsync(client, consultationId);
        var segmentId = Assert.Single(created.Transcript).Id;

        var response = await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/transcript-segments/{segmentId}/exclude",
            new { reason = "Personal information not clinically relevant" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var workspace = await response.Content.ReadFromJsonAsync<WorkspaceResponse>();
        var segment = Assert.Single(workspace!.Transcript);
        Assert.False(segment.IncludeInReasoning);
        Assert.False(segment.IsRedacted);
        Assert.Equal("Tengo la pierna izquierda inchada desde ayer.", segment.OriginalText);
    }
    [Fact]
    public async Task Clinician_can_redact_transcript_from_the_ordinary_clinical_view()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        await LoginAsync(client);
        var consultationId = await context.CreateMiguelDemoConsultationAsync(client);
        (await client.PostAsync($"/api/consultations/{consultationId}/start", null)).EnsureSuccessStatusCode();
        var created = await AddSegmentAsync(client, consultationId);
        var segmentId = Assert.Single(created.Transcript).Id;

        var response = await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/transcript-segments/{segmentId}/redact",
            new { reason = "Patient requested exclusion" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var workspace = await response.Content.ReadFromJsonAsync<WorkspaceResponse>();
        var segment = Assert.Single(workspace!.Transcript);
        Assert.True(segment.IsRedacted);
        Assert.False(segment.IncludeInReasoning);
        Assert.Equal("[Redacted]", segment.OriginalText);
        Assert.Null(segment.TranslatedText);
    }

    [Fact]
    public async Task Transcript_changes_are_audited_without_copying_transcript_text_into_audit_metadata()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        await LoginAsync(client);
        var consultationId = await context.CreateMiguelDemoConsultationAsync(client);
        (await client.PostAsync($"/api/consultations/{consultationId}/start", null)).EnsureSuccessStatusCode();
        var created = await AddSegmentAsync(client, consultationId);
        var segmentId = Assert.Single(created.Transcript).Id;

        (await client.PatchAsJsonAsync(
            $"/api/consultations/{consultationId}/transcript-segments/{segmentId}/correction",
            new { correctedText = "Tengo la pierna izquierda hinchada desde ayer.", translatedText = "My left leg has been swollen since yesterday." })).EnsureSuccessStatusCode();
        (await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/transcript-segments/{segmentId}/speaker",
            new { speakerRole = "Interpreter" })).EnsureSuccessStatusCode();
        (await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/transcript-segments/{segmentId}/exclude",
            new { reason = "Personal information not clinically relevant" })).EnsureSuccessStatusCode();
        (await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/transcript-segments/{segmentId}/redact",
            new { reason = "Patient requested exclusion" })).EnsureSuccessStatusCode();

        await using var scope = context.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ClarityClinicalDbContext>();
        var events = await db.AuditEvents
            .Where(item => item.ConsultationId == consultationId && item.EntityType == "TranscriptSegment")
            .ToArrayAsync();
        var actions = events.Select(item => item.Action).ToArray();

        Assert.Contains("TranscriptCorrected", actions);
        Assert.Contains("TranscriptSpeakerChanged", actions);
        Assert.Contains("TranscriptExcludedFromReasoning", actions);
        Assert.Contains("TranscriptRedacted", actions);

        var auditMetadata = string.Join(" | ", events.Select(item =>
            $"{item.PreviousState} {item.NewState} {item.Reason}"));
        Assert.DoesNotContain("pierna", auditMetadata, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("left leg", auditMetadata, StringComparison.OrdinalIgnoreCase);
    }
    private static async Task<WorkspaceResponse> AddSegmentAsync(HttpClient client, Guid consultationId)
    {
        var response = await client.PostAsJsonAsync($"/api/consultations/{consultationId}/transcript-segments", new
        {
            speakerRole = "Patient", sourceLanguage = "es-ES",
            originalText = "Tengo la pierna izquierda inchada desde ayer.",
            translatedText = "My left leg has been swollen since yesterday.", recognitionConfidence = 0.92
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<WorkspaceResponse>())!;
    }

    private static async Task LoginAsync(HttpClient client)
    {
        (await client.PostAsJsonAsync("/api/auth/demo-login", new { role = "Clinician" })).EnsureSuccessStatusCode();
    }

    private sealed record WorkspaceResponse(IReadOnlyList<TranscriptResponse> Transcript);
    private sealed record TranscriptResponse(Guid Id, string SpeakerRole, string OriginalText, string? TranslatedText, bool Corrected, bool IncludeInReasoning, bool IsRedacted);
}