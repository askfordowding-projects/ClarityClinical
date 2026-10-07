using ClarityClinical.Application.Audit;
using ClarityClinical.Application.ClinicalIntelligence;
using ClarityClinical.Application.Identity;
using ClarityClinical.Domain.Assessments;
using ClarityClinical.Domain.Consultations;

namespace ClarityClinical.Application.Consultations.Responses;

public sealed class RecordClinicianResponseService(
    IConsultationRepository consultationRepository,
    IClinicianResponseRepository responseRepository,
    IClinicalIntelligenceProvider clinicalIntelligenceProvider,
    ICurrentUserAccessor currentUserAccessor,
    IAuditWriter auditWriter)
{
    public async Task<ClinicianResponse> RecordAsync(
        Guid consultationId,
        string recommendationKey,
        ClinicianResponseType responseType,
        string? rationale,
        string? modifiedAction,
        CancellationToken cancellationToken)
    {
        var consultation = await consultationRepository.GetAsync(consultationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Consultation '{consultationId}' was not found.");
        if (consultation.Status != ConsultationStatus.InProgress)
        {
            throw new InvalidOperationException("Clinician responses can only be recorded during an in progress consultation.");
        }

        var intelligence = await clinicalIntelligenceProvider.EvaluateAsync(consultation, cancellationToken);
        if (!intelligence.Candidates.Any(candidate =>
                string.Equals(candidate.Key, recommendationKey, StringComparison.Ordinal)))
        {
            throw new KeyNotFoundException($"Recommendation '{recommendationKey}' was not found.");
        }

        var actor = currentUserAccessor.CurrentUser
            ?? new CurrentUser("system", "System", "System", true);
        var response = new ClinicianResponse(
            Guid.NewGuid(),
            consultationId,
            recommendationKey,
            responseType,
            rationale,
            modifiedAction,
            DateTimeOffset.UtcNow,
            actor.UserId,
            actor.Role);
        await responseRepository.AddAsync(response, cancellationToken);

        var correlationId = Guid.NewGuid().ToString("N");
        await auditWriter.WriteAsync(new AuditWriteRequest(
            consultationId,
            "ClinicianResponseRecorded",
            nameof(ClinicianResponse),
            response.Id.ToString(),
            correlationId,
            NewState: response.ResponseType.ToString(),
            Reason: response.Rationale), cancellationToken);

        return response;
    }
}
