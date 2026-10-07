using System.Net;
using System.Net.Http.Json;
using ClarityClinical.Api.Tests.TestSupport;

namespace ClarityClinical.Api.Tests.Workspace;

public sealed class WorkspaceContractTests
{
    [Fact]
    public async Task Workspace_returns_everything_needed_to_render_the_clinical_screen()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        await LoginAsClinicianAsync(client);
        var consultationId = await context.CreateMiguelDemoConsultationAsync(client);
        await StartConsultationAsync(client, consultationId);

        var response = await client.GetAsync($"/api/consultations/{consultationId}/workspace");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var workspace = await response.Content.ReadFromJsonAsync<WorkspaceResponse>();
        Assert.NotNull(workspace);
        Assert.Equal("InProgress", workspace.Consultation.Status);
        Assert.Equal("Miguel Santos", workspace.Patient.DisplayName);
        Assert.Equal(58, workspace.Patient.Age);
        Assert.Contains(workspace.Patient.Allergies, value => value.DisplayName == "Penicillin");
        Assert.Contains(workspace.Patient.Allergies, value => value.DisplayName == "Ibuprofen");
        Assert.Contains(workspace.Patient.Medications, value => value.DisplayName == "Metformin");
        Assert.Contains(workspace.Assessments, value => value.Key == "dvt");
        Assert.Contains(workspace.Assessments, value => value.Key == "cellulitis");
        Assert.Contains(workspace.Assessments, value => value.Key == "musculoskeletal");
        Assert.True(workspace.Permissions.CanRecordObservation);
        Assert.True(workspace.Permissions.CanCompleteConsultation);
    }

    [Fact]
    public async Task Adding_unilateral_calf_swelling_observation_recalculates_DVT_to_48()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        await LoginAsClinicianAsync(client);
        var consultationId = await context.CreateMiguelDemoConsultationAsync(client);
        await StartConsultationAsync(client, consultationId);

        var response = await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/observations",
            new
            {
                code = "unilateral-calf-swelling",
                displayText = "Unilateral calf swelling"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var workspace = await response.Content.ReadFromJsonAsync<WorkspaceResponse>();
        Assert.NotNull(workspace);
        var dvt = Assert.Single(workspace.Assessments, value => value.Key == "dvt");
        Assert.Equal(48, dvt.CurrentScore);
        Assert.Equal("IllustrativePriority", dvt.ScoreType);
        Assert.All(dvt.SupportingEvidence, evidence => Assert.NotEqual(Guid.Empty, evidence.FactId));
        Assert.Contains(
            dvt.SupportingEvidence,
            evidence => evidence.DisplayText == "Unilateral calf swelling"
                && evidence.Source == "ClinicalObservation");
    }


    [Fact]
    public async Task Excluding_prolonged_travel_removes_its_DVT_contribution()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        await LoginAsClinicianAsync(client);
        var consultationId = await context.CreateMiguelDemoConsultationAsync(client);
        await StartConsultationAsync(client, consultationId);

        await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/observations",
            new { code = "unilateral-calf-swelling", displayText = "Unilateral calf swelling" });
        var travelResponse = await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/patient-reports",
            new { code = "recent-prolonged-travel", displayText = "Recent prolonged travel" });
        travelResponse.EnsureSuccessStatusCode();
        var before = await travelResponse.Content.ReadFromJsonAsync<WorkspaceResponse>();
        var dvtBefore = Assert.Single(before!.Assessments, value => value.Key == "dvt");
        Assert.Equal(67, dvtBefore.CurrentScore);
        var travelFact = Assert.Single(
            dvtBefore.SupportingEvidence,
            evidence => evidence.FactCode == "recent-prolonged-travel");
        Assert.NotEqual(Guid.Empty, travelFact.FactId);

        var excludeResponse = await client.PostAsync(
            $"/api/consultations/{consultationId}/clinical-facts/{travelFact.FactId}/exclude",
            null);

        Assert.Equal(HttpStatusCode.OK, excludeResponse.StatusCode);
        var after = await excludeResponse.Content.ReadFromJsonAsync<WorkspaceResponse>();
        var dvtAfter = Assert.Single(after!.Assessments, value => value.Key == "dvt");
        Assert.Equal(48, dvtAfter.CurrentScore);
        Assert.Equal(67, dvtAfter.PreviousScore);
        Assert.Contains("removed from clinical reasoning", dvtAfter.ChangeReason);
    }
    private static async Task LoginAsClinicianAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/demo-login",
            new { role = "Clinician" });
        response.EnsureSuccessStatusCode();
    }

    private static async Task StartConsultationAsync(HttpClient client, Guid consultationId)
    {
        var response = await client.PostAsync($"/api/consultations/{consultationId}/start", null);
        response.EnsureSuccessStatusCode();
    }

    private sealed record WorkspaceResponse(
        ConsultationResponse Consultation,
        PatientResponse Patient,
        IReadOnlyList<AssessmentResponse> Assessments,
        PermissionsResponse Permissions);

    private sealed record ConsultationResponse(Guid Id, string Status);
    private sealed record PatientResponse(
        Guid Id,
        string DisplayName,
        int Age,
        IReadOnlyList<NamedValueResponse> Conditions,
        IReadOnlyList<NamedValueResponse> Allergies,
        IReadOnlyList<NamedValueResponse> Medications);
    private sealed record NamedValueResponse(string Code, string DisplayName);
    private sealed record AssessmentResponse(
        string Key,
        string DisplayName,
        int CurrentScore,
        int? PreviousScore,
        string ScoreType,
        IReadOnlyList<EvidenceResponse> SupportingEvidence,
        IReadOnlyList<string> MissingEvidence,
        IReadOnlyList<string> SuggestedChecks,
        IReadOnlyList<string> Warnings,
        string? ChangeReason);
    private sealed record EvidenceResponse(Guid FactId, string FactCode, string DisplayText, string Source);
    private sealed record PermissionsResponse(
        bool CanRecordObservation,
        bool CanAddPatientReport,
        bool CanCompleteConsultation);
}
