using System.Net;
using System.Net.Http.Json;
using ClarityClinical.Api.Tests.TestSupport;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClarityClinical.Api.Tests.Acceptance;

public sealed class MiguelVerticalSliceTests
{
    [Fact]
    public async Task Miguel_scenario_runs_end_to_end_with_explainable_reversible_assessment()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();

        (await client.PostAsJsonAsync("/api/auth/demo-login", new { role = "Clinician" }))
            .EnsureSuccessStatusCode();
        var consultationId = await context.CreateMiguelDemoConsultationAsync(client);
        (await client.PostAsync($"/api/consultations/{consultationId}/start", null))
            .EnsureSuccessStatusCode();

        var swelling = await AddObservationAsync(
            client,
            consultationId,
            "unilateral-calf-swelling",
            "Unilateral calf swelling");
        AssertScore(swelling, "dvt", 48);

        var travel = await AddPatientReportAsync(
            client,
            consultationId,
            "recent-prolonged-travel",
            "Recent prolonged travel");
        var dvtAfterTravel = AssertScore(travel, "dvt", 67);
        var travelFact = Assert.Single(
            dvtAfterTravel.SupportingEvidence,
            evidence => evidence.FactCode == "recent-prolonged-travel");

        var asymmetry = await AddObservationAsync(
            client,
            consultationId,
            "calf-asymmetry",
            "Calf asymmetry");
        AssertScore(asymmetry, "dvt", 72);

        var wound = await AddObservationAsync(
            client,
            consultationId,
            "ankle-wound-warmth",
            "Ankle wound with warmth");
        AssertScore(wound, "cellulitis", 58);

        var exclude = await client.PostAsync(
            $"/api/consultations/{consultationId}/clinical-facts/{travelFact.FactId}/exclude",
            null);
        Assert.Equal(HttpStatusCode.OK, exclude.StatusCode);
        var afterExclusion = await exclude.Content.ReadFromJsonAsync<WorkspaceResponse>();
        var dvtAfterExclusion = Assert.Single(afterExclusion!.Assessments, item => item.Key == "dvt");
        Assert.True(dvtAfterExclusion.CurrentScore < 72);
        Assert.Contains("removed from clinical reasoning", dvtAfterExclusion.ChangeReason);

        var clinicianResponse = await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/recommendations/dvt/responses",
            new
            {
                responseType = "Modified",
                rationale = "Travel history requires confirmation",
                modifiedAction = "Complete Wells assessment before escalation"
            });
        Assert.Equal(HttpStatusCode.OK, clinicianResponse.StatusCode);

        var complete = await client.PostAsync(
            $"/api/consultations/{consultationId}/complete",
            null);
        Assert.Equal(HttpStatusCode.OK, complete.StatusCode);

        var finalWorkspace = await client.GetFromJsonAsync<WorkspaceResponse>(
            $"/api/consultations/{consultationId}/workspace");
        Assert.Equal("Completed", finalWorkspace!.Consultation.Status);
        var finalDvt = Assert.Single(finalWorkspace.Assessments, item => item.Key == "dvt");
        Assert.Equal("Modified", finalDvt.Response?.ResponseType);

        await using var scope = context.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ClarityClinicalDbContext>();
        var auditActions = await dbContext.AuditEvents
            .Where(item => item.ConsultationId == consultationId)
            .Select(item => item.Action)
            .ToListAsync();

        Assert.Contains("ClinicalFactAdded", auditActions);
        Assert.Contains("RuleEvaluated", auditActions);
        Assert.Contains("AssessmentChanged", auditActions);
        Assert.Contains("ClinicalFactExcluded", auditActions);
        Assert.Contains("ClinicianResponseRecorded", auditActions);
    }

    private static async Task<WorkspaceResponse> AddObservationAsync(
        HttpClient client,
        Guid consultationId,
        string code,
        string displayText)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/observations",
            new { code, displayText });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<WorkspaceResponse>())!;
    }

    private static async Task<WorkspaceResponse> AddPatientReportAsync(
        HttpClient client,
        Guid consultationId,
        string code,
        string displayText)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/patient-reports",
            new { code, displayText });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<WorkspaceResponse>())!;
    }

    private static AssessmentResponse AssertScore(
        WorkspaceResponse workspace,
        string key,
        int expected)
    {
        var candidate = Assert.Single(workspace.Assessments, item => item.Key == key);
        Assert.Equal(expected, candidate.CurrentScore);
        return candidate;
    }

    private sealed record WorkspaceResponse(
        ConsultationResponse Consultation,
        IReadOnlyList<AssessmentResponse> Assessments);
    private sealed record ConsultationResponse(Guid Id, string Status);
    private sealed record AssessmentResponse(
        string Key,
        int CurrentScore,
        int? PreviousScore,
        IReadOnlyList<EvidenceResponse> SupportingEvidence,
        string? ChangeReason,
        ClinicianResponseBody? Response);
    private sealed record EvidenceResponse(
        Guid FactId,
        string FactCode,
        string DisplayText,
        string Source);
    private sealed record ClinicianResponseBody(
        string ResponseType,
        string? Rationale,
        string? ModifiedAction);
}
