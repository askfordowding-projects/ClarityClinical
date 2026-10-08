namespace ClarityClinical.Domain.Demo;

public sealed class DemoScenario
{
    private readonly List<DemoScenarioStep> _steps = [];

    public DemoScenario(
        Guid id,
        string scenarioKey,
        string name,
        string description,
        Guid patientId,
        string setting,
        string sourceLanguage,
        string clinicianLanguage,
        bool isCanonical)
    {
        if (string.IsNullOrWhiteSpace(scenarioKey))
        {
            throw new ArgumentException("Scenario key is required.", nameof(scenarioKey));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Scenario name is required.", nameof(name));
        }

        Id = id;
        ScenarioKey = scenarioKey;
        Name = name;
        Description = description;
        PatientId = patientId;
        Setting = setting;
        SourceLanguage = sourceLanguage;
        ClinicianLanguage = clinicianLanguage;
        IsCanonical = isCanonical;
    }

    public Guid Id { get; }
    public string ScenarioKey { get; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Guid PatientId { get; }
    public string Setting { get; private set; }
    public string SourceLanguage { get; private set; }
    public string ClinicianLanguage { get; private set; }
    public bool IsCanonical { get; }
    public IReadOnlyCollection<DemoScenarioStep> Steps => _steps.AsReadOnly();

    public void UpdateDetails(
        string name,
        string description,
        string setting,
        string sourceLanguage,
        string clinicianLanguage)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Scenario name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(setting)) throw new ArgumentException("Setting is required.", nameof(setting));
        if (string.IsNullOrWhiteSpace(sourceLanguage)) throw new ArgumentException("Source language is required.", nameof(sourceLanguage));
        if (string.IsNullOrWhiteSpace(clinicianLanguage)) throw new ArgumentException("Clinician language is required.", nameof(clinicianLanguage));
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Setting = setting.Trim();
        SourceLanguage = sourceLanguage.Trim();
        ClinicianLanguage = clinicianLanguage.Trim();
    }

    public void AddStep(DemoScenarioStep step)
    {
        ArgumentNullException.ThrowIfNull(step);
        if (step.ScenarioId != Id)
        {
            throw new InvalidOperationException("Scenario step belongs to a different scenario.");
        }

        _steps.Add(step);
    }
}
