namespace ClarityClinical.Domain.Consultations;

public sealed class ClinicalFact
{
    public ClinicalFact(
        Guid id,
        string code,
        string displayText,
        ClinicalFactSource source,
        DateTimeOffset occurredAt,
        Guid? sourceEventId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Clinical fact code is required.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(displayText))
        {
            throw new ArgumentException("Clinical fact display text is required.", nameof(displayText));
        }

        Id = id;
        Code = code;
        DisplayText = displayText;
        Source = source;
        OccurredAt = occurredAt;
        SourceEventId = sourceEventId;
        IncludeInReasoning = true;
    }

    public Guid Id { get; }
    public string Code { get; }
    public string DisplayText { get; }
    public ClinicalFactSource Source { get; }
    public DateTimeOffset OccurredAt { get; }
    public Guid? SourceEventId { get; }
    public bool IncludeInReasoning { get; private set; }

    public void ExcludeFromReasoning()
    {
        IncludeInReasoning = false;
    }

    public void RestoreToReasoning()
    {
        IncludeInReasoning = true;
    }
}
