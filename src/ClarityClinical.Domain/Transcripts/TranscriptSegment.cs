namespace ClarityClinical.Domain.Transcripts;

public sealed class TranscriptSegment
{
    public TranscriptSegment(
        Guid id,
        Guid consultationId,
        ConsultationSpeakerRole speakerRole,
        string sourceLanguage,
        string machineOriginalText,
        string? translatedText,
        double? recognitionConfidence,
        DateTimeOffset occurredAt)
    {
        if (string.IsNullOrWhiteSpace(sourceLanguage))
        {
            throw new ArgumentException("Source language is required.", nameof(sourceLanguage));
        }

        if (string.IsNullOrWhiteSpace(machineOriginalText))
        {
            throw new ArgumentException("Transcript text is required.", nameof(machineOriginalText));
        }

        if (recognitionConfidence is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(recognitionConfidence),
                "Recognition confidence must be between 0 and 1.");
        }

        Id = id;
        ConsultationId = consultationId;
        SpeakerRole = speakerRole;
        SourceLanguage = sourceLanguage.Trim();
        MachineOriginalText = machineOriginalText.Trim();
        TranslatedText = string.IsNullOrWhiteSpace(translatedText) ? null : translatedText.Trim();
        RecognitionConfidence = recognitionConfidence;
        OccurredAt = occurredAt;
        IncludeInReasoning = true;
    }

    public Guid Id { get; }
    public Guid ConsultationId { get; }
    public ConsultationSpeakerRole SpeakerRole { get; private set; }
    public string SourceLanguage { get; }
    public string MachineOriginalText { get; }
    public string? CorrectedText { get; private set; }
    public string DisplayOriginalText => CorrectedText ?? MachineOriginalText;
    public string? TranslatedText { get; private set; }
    public double? RecognitionConfidence { get; }
    public DateTimeOffset OccurredAt { get; }
    public bool Corrected => CorrectedText is not null;
    public bool IncludeInReasoning { get; private set; }
    public bool IsRedacted { get; private set; }

    public void Correct(string correctedText, string? translatedText)
    {
        if (string.IsNullOrWhiteSpace(correctedText))
        {
            throw new ArgumentException("Corrected transcript text is required.", nameof(correctedText));
        }

        CorrectedText = correctedText.Trim();
        TranslatedText = string.IsNullOrWhiteSpace(translatedText) ? null : translatedText.Trim();
    }

    public void ChangeSpeaker(ConsultationSpeakerRole speakerRole)
    {
        SpeakerRole = speakerRole;
    }

    public void ExcludeFromReasoning()
    {
        IncludeInReasoning = false;
    }

    public void Redact()
    {
        IsRedacted = true;
        IncludeInReasoning = false;
    }}
