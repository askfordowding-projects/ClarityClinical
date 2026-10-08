using ClarityClinical.Domain.Transcripts;

namespace ClarityClinical.Domain.Tests.Transcripts;

public sealed class TranscriptSegmentTests
{
    [Fact]
    public void Correct_preserves_machine_original_and_uses_corrected_text_for_display()
    {
        var segment = CreateSegment();

        segment.Correct(
            "Tengo la pierna izquierda hinchada desde ayer.",
            "My left leg has been swollen since yesterday.");

        Assert.Equal("Tengo la pierna izquierda inchada desde ayer.", segment.MachineOriginalText);
        Assert.Equal("Tengo la pierna izquierda hinchada desde ayer.", segment.CorrectedText);
        Assert.Equal("Tengo la pierna izquierda hinchada desde ayer.", segment.DisplayOriginalText);
        Assert.Equal("My left leg has been swollen since yesterday.", segment.TranslatedText);
        Assert.True(segment.Corrected);
    }

    [Fact]
    public void ChangeSpeaker_updates_attribution_without_changing_text()
    {
        var segment = CreateSegment();

        segment.ChangeSpeaker(ConsultationSpeakerRole.Interpreter);

        Assert.Equal(ConsultationSpeakerRole.Interpreter, segment.SpeakerRole);
        Assert.Equal("Tengo la pierna izquierda inchada desde ayer.", segment.MachineOriginalText);
    }

    [Fact]
    public void ExcludeFromReasoning_keeps_segment_visible_but_marks_it_unavailable_to_reasoning()
    {
        var segment = CreateSegment();

        segment.ExcludeFromReasoning();

        Assert.False(segment.IncludeInReasoning);
        Assert.False(segment.IsRedacted);
    }

    [Fact]
    public void Redact_hides_segment_from_reasoning_without_destroying_original()
    {
        var segment = CreateSegment();

        segment.Redact();

        Assert.True(segment.IsRedacted);
        Assert.False(segment.IncludeInReasoning);
        Assert.Equal("Tengo la pierna izquierda inchada desde ayer.", segment.MachineOriginalText);
    }

    private static TranscriptSegment CreateSegment() => new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        ConsultationSpeakerRole.Patient,
        "es-ES",
        "Tengo la pierna izquierda inchada desde ayer.",
        "My left leg has been swollen since yesterday.",
        0.92,
        DateTimeOffset.UtcNow);
}