using ClarityClinical.Domain.Assessments;

namespace ClarityClinical.Domain.Tests.Assessments;

public sealed class AssessmentCandidateTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Priority_score_must_be_between_zero_and_one_hundred(int score)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new AssessmentCandidate("dvt", "Deep vein thrombosis", score));
    }

    [Fact]
    public void Candidate_uses_illustrative_priority_score_type()
    {
        var candidate = new AssessmentCandidate("dvt", "Deep vein thrombosis", 48);

        Assert.Equal(PriorityScoreType.IllustrativePriority, candidate.ScoreType);
        Assert.Equal(48, candidate.CurrentScore);
        Assert.Null(candidate.PreviousScore);
    }

    [Fact]
    public void Updating_score_preserves_previous_score_and_reasons()
    {
        var candidate = new AssessmentCandidate("dvt", "Deep vein thrombosis", 48);

        candidate.UpdateScore(67, ["Recent prolonged travel"]);

        Assert.Equal(48, candidate.PreviousScore);
        Assert.Equal(67, candidate.CurrentScore);
        Assert.Equal(["Recent prolonged travel"], candidate.ChangeReasons);
    }
}
