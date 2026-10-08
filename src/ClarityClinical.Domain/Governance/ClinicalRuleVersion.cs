namespace ClarityClinical.Domain.Governance;

public sealed class ClinicalRuleVersion
{
    private ClinicalRuleVersion()
    {
        RuleKey = string.Empty;
        DisplayName = string.Empty;
        Version = string.Empty;
        Status = string.Empty;
        ScoreType = string.Empty;
        ProvenanceNote = string.Empty;
    }

    public ClinicalRuleVersion(
        Guid id,
        string ruleKey,
        string displayName,
        string version,
        string status,
        string scoreType,
        bool isIllustrative,
        string provenanceNote,
        Guid guidelineSourceId,
        DateOnly effectiveFrom)
    {
        Id = id;
        RuleKey = Required(ruleKey, nameof(ruleKey));
        DisplayName = Required(displayName, nameof(displayName));
        Version = Required(version, nameof(version));
        Status = Required(status, nameof(status));
        ScoreType = Required(scoreType, nameof(scoreType));
        IsIllustrative = isIllustrative;
        ProvenanceNote = Required(provenanceNote, nameof(provenanceNote));
        GuidelineSourceId = guidelineSourceId;
        EffectiveFrom = effectiveFrom;
    }

    public Guid Id { get; private set; }
    public string RuleKey { get; private set; }
    public string DisplayName { get; private set; }
    public string Version { get; private set; }
    public string Status { get; private set; }
    public string ScoreType { get; private set; }
    public bool IsIllustrative { get; private set; }
    public string ProvenanceNote { get; private set; }
    public Guid GuidelineSourceId { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{name} is required.", name)
            : value.Trim();
}
