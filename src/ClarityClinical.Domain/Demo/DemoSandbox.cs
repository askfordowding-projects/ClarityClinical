namespace ClarityClinical.Domain.Demo;

public sealed class DemoSandbox
{
    private DemoSandbox()
    {
        VisitorId = string.Empty;
    }

    public DemoSandbox(
        Guid id,
        string visitorId,
        Guid sourceScenarioId,
        Guid sandboxScenarioId,
        Guid sandboxPatientId,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(visitorId))
        {
            throw new ArgumentException("Visitor id is required.", nameof(visitorId));
        }

        Id = id;
        VisitorId = visitorId.Trim();
        SourceScenarioId = sourceScenarioId;
        SandboxScenarioId = sandboxScenarioId;
        SandboxPatientId = sandboxPatientId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string VisitorId { get; private set; }
    public Guid SourceScenarioId { get; private set; }
    public Guid SandboxScenarioId { get; private set; }
    public Guid SandboxPatientId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Touch(DateTimeOffset now) => UpdatedAt = now;
}
