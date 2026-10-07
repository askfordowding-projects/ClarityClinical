using System.Net;
using System.Net.Http.Json;
using ClarityClinical.Api.Tests.TestSupport;

namespace ClarityClinical.Api.Tests.Consultations;

public sealed class ClinicianResponseTests
{
    [Theory]
    [InlineData("Accepted")]
    [InlineData("Rejected")]
    [InlineData("Deferred")]
    public async Task Clinician_can_record_non_modified_response(string responseType)
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        await LoginAsync(client);
        var consultationId = await context.CreateMiguelDemoConsultationAsync(client);
        await client.PostAsync($"/api/consultations/{consultationId}/start", null);

        var response = await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/recommendations/dvt/responses",
            new { responseType, rationale = "Clinical review completed", modifiedAction = (string?)null });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ResponseBody>();
        Assert.Equal(responseType, body?.ResponseType);
        Assert.Equal("dvt", body?.RecommendationKey);
    }

    [Fact]
    public async Task Modified_response_persists_rationale_and_changed_action()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        await LoginAsync(client);
        var consultationId = await context.CreateMiguelDemoConsultationAsync(client);
        await client.PostAsync($"/api/consultations/{consultationId}/start", null);

        var response = await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/recommendations/dvt/responses",
            new
            {
                responseType = "Modified",
                rationale = "Travel history is uncertain",
                modifiedAction = "Complete Wells assessment before escalation"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ResponseBody>();
        Assert.Equal("Modified", body?.ResponseType);
        Assert.Equal("Travel history is uncertain", body?.Rationale);
        Assert.Equal("Complete Wells assessment before escalation", body?.ModifiedAction);
    }

    [Fact]
    public async Task Modified_response_without_rationale_or_action_is_rejected()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        await LoginAsync(client);
        var consultationId = await context.CreateMiguelDemoConsultationAsync(client);
        await client.PostAsync($"/api/consultations/{consultationId}/start", null);

        var response = await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/recommendations/dvt/responses",
            new { responseType = "Modified", rationale = "", modifiedAction = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Latest_response_is_returned_with_workspace_candidate()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        await LoginAsync(client);
        var consultationId = await context.CreateMiguelDemoConsultationAsync(client);
        await client.PostAsync($"/api/consultations/{consultationId}/start", null);
        await client.PostAsJsonAsync(
            $"/api/consultations/{consultationId}/recommendations/dvt/responses",
            new { responseType = "Accepted", rationale = "Reviewed", modifiedAction = (string?)null });

        var workspace = await client.GetFromJsonAsync<WorkspaceBody>(
            $"/api/consultations/{consultationId}/workspace");

        var dvt = Assert.Single(workspace!.Assessments, item => item.Key == "dvt");
        Assert.NotNull(dvt.Response);
        Assert.Equal("Accepted", dvt.Response.ResponseType);
        Assert.Equal("Reviewed", dvt.Response.Rationale);
    }

    private static async Task LoginAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/demo-login", new { role = "Clinician" });
        response.EnsureSuccessStatusCode();
    }

    private sealed record WorkspaceBody(IReadOnlyList<AssessmentBody> Assessments);
    private sealed record AssessmentBody(
        string Key,
        ClinicianResponseBody? Response);
    private sealed record ClinicianResponseBody(
        string ResponseType,
        string? Rationale,
        string? ModifiedAction);

    private sealed record ResponseBody(
        Guid Id,
        string RecommendationKey,
        string ResponseType,
        string? Rationale,
        string? ModifiedAction);
}
