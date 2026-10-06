using ClarityClinical.Domain.Consultations;

namespace ClarityClinical.Domain.Tests.Consultations;

public sealed class ConsultationTests
{
    [Fact]
    public void New_consultation_is_not_started()
    {
        var consultation = new Consultation(Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal(ConsultationStatus.NotStarted, consultation.Status);
        Assert.Null(consultation.StartedAt);
        Assert.Null(consultation.CompletedAt);
    }

    [Fact]
    public void Start_changes_status_to_in_progress()
    {
        var consultation = new Consultation(Guid.NewGuid(), Guid.NewGuid());
        var now = DateTimeOffset.UtcNow;

        consultation.Start(now);

        Assert.Equal(ConsultationStatus.InProgress, consultation.Status);
        Assert.Equal(now, consultation.StartedAt);
    }

    [Fact]
    public void Complete_requires_in_progress_consultation()
    {
        var consultation = new Consultation(Guid.NewGuid(), Guid.NewGuid());

        var exception = Assert.Throws<InvalidOperationException>(() =>
            consultation.Complete(DateTimeOffset.UtcNow));

        Assert.Contains("in progress", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Completed_consultation_cannot_be_started_again()
    {
        var consultation = new Consultation(Guid.NewGuid(), Guid.NewGuid());
        consultation.Start(DateTimeOffset.UtcNow);
        consultation.Complete(DateTimeOffset.UtcNow.AddMinutes(10));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            consultation.Start(DateTimeOffset.UtcNow.AddMinutes(20)));

        Assert.Contains("not started", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
