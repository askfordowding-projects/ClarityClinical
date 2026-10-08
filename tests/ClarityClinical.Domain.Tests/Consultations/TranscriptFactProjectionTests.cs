using ClarityClinical.Domain.Consultations;

namespace ClarityClinical.Domain.Tests.Consultations;

public sealed class TranscriptFactProjectionTests
{
    [Fact]
    public void Synchronizing_without_a_previously_projected_fact_retains_history_but_excludes_it()
    {
        var consultation = new Consultation(Guid.NewGuid(), Guid.NewGuid());
        var sourceEventId = Guid.NewGuid();
        var fact = new ClinicalFact(
            Guid.NewGuid(),
            "unilateral-calf-swelling",
            "Unilateral calf swelling",
            ClinicalFactSource.PatientReport,
            DateTimeOffset.UtcNow,
            sourceEventId);
        consultation.AddFact(fact);

        consultation.SynchronizeFactsFromEvent(sourceEventId, []);

        var historical = Assert.Single(consultation.Facts);
        Assert.Equal(fact.Id, historical.Id);
        Assert.False(historical.IncludeInReasoning);
    }

    [Fact]
    public void Synchronizing_the_same_fact_restores_the_existing_fact_identity()
    {
        var consultation = new Consultation(Guid.NewGuid(), Guid.NewGuid());
        var sourceEventId = Guid.NewGuid();
        var fact = new ClinicalFact(
            Guid.NewGuid(),
            "unilateral-calf-swelling",
            "Unilateral calf swelling",
            ClinicalFactSource.PatientReport,
            DateTimeOffset.UtcNow,
            sourceEventId);
        consultation.AddFact(fact);
        consultation.SynchronizeFactsFromEvent(sourceEventId, []);

        consultation.SynchronizeFactsFromEvent(sourceEventId,
        [
            new ClinicalFact(
                Guid.NewGuid(),
                "unilateral-calf-swelling",
                "Unilateral calf swelling",
                ClinicalFactSource.PatientReport,
                DateTimeOffset.UtcNow,
                sourceEventId)
        ]);

        var restored = Assert.Single(consultation.Facts);
        Assert.Equal(fact.Id, restored.Id);
        Assert.True(restored.IncludeInReasoning);
    }
}
