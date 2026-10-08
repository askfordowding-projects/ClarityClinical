using System.Globalization;
using System.Text;
using ClarityClinical.Domain.Consultations;
using ClarityClinical.Domain.Transcripts;

namespace ClarityClinical.Application.Transcripts;

public sealed class TranscriptClinicalFactExtractor
{
    public IReadOnlyList<ClinicalFact> Extract(TranscriptSegment segment)
    {
        ArgumentNullException.ThrowIfNull(segment);

        if (!segment.IncludeInReasoning || segment.IsRedacted)
        {
            return [];
        }

        var source = segment.SpeakerRole switch
        {
            ConsultationSpeakerRole.Patient => ClinicalFactSource.PatientReport,
            ConsultationSpeakerRole.Clinician => ClinicalFactSource.ClinicalObservation,
            _ => (ClinicalFactSource?)null
        };

        if (source is null)
        {
            return [];
        }

        var searchable = Normalise($"{segment.DisplayOriginalText} {segment.TranslatedText}");
        var facts = new List<ClinicalFact>();

        AddIf(
            facts,
            segment,
            source.Value,
            "unilateral-calf-swelling",
            "Unilateral calf swelling",
            ContainsAny(searchable,
                "left leg has been swollen",
                "left leg is swollen",
                "left calf is swollen",
                "unilateral calf swelling",
                "pierna izquierda hinchada",
                "pantorrilla izquierda hinchada"));

        AddIf(
            facts,
            segment,
            source.Value,
            "recent-prolonged-travel",
            "Recent prolonged travel",
            ContainsAny(searchable,
                "eight hour flight",
                "8 hour flight",
                "prolonged travel",
                "long haul flight",
                "vuelo de ocho horas",
                "viaje prolongado"));

        AddIf(
            facts,
            segment,
            source.Value,
            "calf-asymmetry",
            "Calf asymmetry",
            ContainsAny(searchable,
                "calf asymmetry",
                "left calf is larger",
                "left calf larger",
                "asymmetry of the calf"));

        AddIf(
            facts,
            segment,
            source.Value,
            "ankle-wound-warmth",
            "Warm ankle wound",
            ContainsAny(searchable,
                "ankle wound is warm",
                "warm ankle wound",
                "wound around the ankle is warm",
                "herida del tobillo esta caliente"));

        AddIf(
            facts,
            segment,
            source.Value,
            "knee-pain",
            "Knee pain",
            ContainsAny(searchable, "knee pain", "dolor de rodilla"));

        AddIf(
            facts,
            segment,
            source.Value,
            "restricted-mobility",
            "Restricted mobility",
            ContainsAny(searchable,
                "restricted mobility",
                "difficulty walking",
                "hard to walk",
                "dificultad para caminar"));

        return facts;
    }

    private static void AddIf(
        ICollection<ClinicalFact> facts,
        TranscriptSegment segment,
        ClinicalFactSource source,
        string code,
        string displayText,
        bool condition)
    {
        if (!condition)
        {
            return;
        }

        facts.Add(new ClinicalFact(
            Guid.NewGuid(),
            code,
            displayText,
            source,
            segment.OccurredAt,
            segment.Id));
    }

    private static bool ContainsAny(string text, params string[] phrases) =>
        phrases.Any(text.Contains);

    private static string Normalise(string value)
    {
        var decomposed = value
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace('-', ' ')
            .Replace("  ", " ");
    }
}
