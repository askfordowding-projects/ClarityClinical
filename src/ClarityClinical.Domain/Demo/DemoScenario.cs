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
    public string Name { get; }
    public string Description { get; }
    public Guid PatientId { get; }
    public string Setting { get; }
    public string SourceLanguage { get; }
    public string ClinicianLanguage { get; }
    public bool IsCanonical { get; }
    public IReadOnlyCollection<DemoScenarioStep> Steps => _steps.AsReadOnly();

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
