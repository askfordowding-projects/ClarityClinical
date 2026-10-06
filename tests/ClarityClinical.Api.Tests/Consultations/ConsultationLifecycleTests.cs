using System.Net;
using System.Net.Http.Json;
using ClarityClinical.Api.Tests.TestSupport;

namespace ClarityClinical.Api.Tests.Consultations;

public sealed class ConsultationLifecycleTests
{
    [Fact]
    public async Task Unauthenticated_start_request_is_rejected()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        var consultationId = await context.AddConsultationAsync();

        var response = await client.PostAsync(
            $"/api/consultations/{consultationId}/start",
            content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Clinician_can_start_and_complete_consultation()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        await LoginAsClinicianAsync(client);
        var consultationId = await context.AddConsultationAsync();

        var start = await client.PostAsync($"/api/consultations/{consultationId}/start", null);
        Assert.Equal(HttpStatusCode.OK, start.StatusCode);
        var startBody = await start.Content.ReadFromJsonAsync<ConsultationStatusResponse>();
        Assert.Equal("InProgress", startBody?.Status);

        var complete = await client.PostAsync($"/api/consultations/{consultationId}/complete", null);
        Assert.Equal(HttpStatusCode.OK, complete.StatusCode);
        var completeBody = await complete.Content.ReadFromJsonAsync<ConsultationStatusResponse>();
        Assert.Equal("Completed", completeBody?.Status);
    }

    [Fact]
    public async Task Completing_unstarted_consultation_returns_validation_problem()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        await LoginAsClinicianAsync(client);
        var consultationId = await context.AddConsultationAsync();

        var response = await client.PostAsync($"/api/consultations/{consultationId}/complete", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Starting_completed_consultation_returns_validation_problem()
    {
        await using var context = await ApiTestContext.CreateAsync();
        using var client = context.CreateClient();
        await LoginAsClinicianAsync(client);
        var consultationId = await context.AddConsultationAsync();

        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PostAsync($"/api/consultations/{consultationId}/start", null)).StatusCode);
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PostAsync($"/api/consultations/{consultationId}/complete", null)).StatusCode);

        var response = await client.PostAsync($"/api/consultations/{consultationId}/start", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task LoginAsClinicianAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/demo-login",
            new { role = "Clinician" });
        response.EnsureSuccessStatusCode();
    }

    private sealed record ConsultationStatusResponse(Guid Id, string Status);
}
