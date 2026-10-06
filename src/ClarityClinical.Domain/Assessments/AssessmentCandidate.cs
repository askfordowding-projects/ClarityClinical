namespace ClarityClinical.Domain.Assessments;

public sealed class AssessmentCandidate
{
    private readonly List<string> _changeReasons = [];

    public AssessmentCandidate(string key, string displayName, int currentScore)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Assessment key is required.", nameof(key));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Assessment display name is required.", nameof(displayName));
        }

        ValidateScore(currentScore);
        Key = key;
        DisplayName = displayName;
        CurrentScore = currentScore;
        ScoreType = PriorityScoreType.IllustrativePriority;
    }

    public string Key { get; }
    public string DisplayName { get; }
    public int CurrentScore { get; private set; }
    public int? PreviousScore { get; private set; }
    public PriorityScoreType ScoreType { get; }
    public IReadOnlyCollection<string> ChangeReasons => _changeReasons.AsReadOnly();

    public void UpdateScore(int score, IEnumerable<string> reasons)
    {
        ValidateScore(score);
        ArgumentNullException.ThrowIfNull(reasons);

        PreviousScore = CurrentScore;
        CurrentScore = score;
        _changeReasons.Clear();
        _changeReasons.AddRange(reasons.Where(reason => !string.IsNullOrWhiteSpace(reason)));
    }

    private static void ValidateScore(int score)
    {
        if (score is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(score), score, "Priority score must be between 0 and 100.");
        }
    }
}
