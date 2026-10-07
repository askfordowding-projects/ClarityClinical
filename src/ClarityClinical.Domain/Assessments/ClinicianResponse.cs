namespace ClarityClinical.Domain.Assessments;

public sealed class ClinicianResponse
{
    public ClinicianResponse(
        Guid id,
        Guid consultationId,
        string recommendationKey,
        ClinicianResponseType responseType,
        string? rationale,
        string? modifiedAction,
        DateTimeOffset respondedAt,
        string actorId,
        string actorRole)
    {
        if (string.IsNullOrWhiteSpace(recommendationKey))
        {
            throw new ArgumentException("Recommendation key is required.", nameof(recommendationKey));
        }

        if (responseType == ClinicianResponseType.NotReviewed)
        {
            throw new ArgumentException("NotReviewed cannot be recorded as a clinician response.", nameof(responseType));
        }

        if (responseType == ClinicianResponseType.Modified
            && string.IsNullOrWhiteSpace(rationale)
            && string.IsNullOrWhiteSpace(modifiedAction))
        {
            throw new ArgumentException(
                "A modified response requires a rationale or modified action.",
                nameof(responseType));
        }

        Id = id;
        ConsultationId = consultationId;
        RecommendationKey = recommendationKey;
        ResponseType = responseType;
        Rationale = string.IsNullOrWhiteSpace(rationale) ? null : rationale.Trim();
        ModifiedAction = string.IsNullOrWhiteSpace(modifiedAction) ? null : modifiedAction.Trim();
        RespondedAt = respondedAt;
        ActorId = actorId;
        ActorRole = actorRole;
    }

    public Guid Id { get; }
    public Guid ConsultationId { get; }
    public string RecommendationKey { get; }
    public ClinicianResponseType ResponseType { get; }
    public string? Rationale { get; }
    public string? ModifiedAction { get; }
    public DateTimeOffset RespondedAt { get; }
    public string ActorId { get; }
    public string ActorRole { get; }
}
