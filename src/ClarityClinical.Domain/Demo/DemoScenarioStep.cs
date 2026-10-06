namespace ClarityClinical.Domain.Demo;

public sealed class DemoScenarioStep
{
    public DemoScenarioStep(
        Guid id,
        Guid scenarioId,
        int sequence,
        DemoScenarioStepType stepType,
        string clinicalFactCode,
        string clinicalFactDisplayText,
        string? originalText = null,
        string? sourceLanguage = null)
    {
        if (sequence < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(sequence));
        }

        if (string.IsNullOrWhiteSpace(clinicalFactCode))
        {
            throw new ArgumentException("Clinical fact code is required.", nameof(clinicalFactCode));
        }

        if (string.IsNullOrWhiteSpace(clinicalFactDisplayText))
        {
            throw new ArgumentException("Clinical fact display text is required.", nameof(clinicalFactDisplayText));
        }

        Id = id;
        ScenarioId = scenarioId;
        Sequence = sequence;
        StepType = stepType;
        ClinicalFactCode = clinicalFactCode;
        ClinicalFactDisplayText = clinicalFactDisplayText;
        OriginalText = originalText;
        SourceLanguage = sourceLanguage;
    }

    public Guid Id { get; }
    public Guid ScenarioId { get; }
    public int Sequence { get; }
    public DemoScenarioStepType StepType { get; }
    public string ClinicalFactCode { get; }
    public string ClinicalFactDisplayText { get; }
    public string? OriginalText { get; }
    public string? SourceLanguage { get; }
}
