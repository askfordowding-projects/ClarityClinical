namespace ClarityClinical.Domain.Governance;

public sealed class GuidelineSource
{
    private GuidelineSource()
    {
        ReferenceCode = string.Empty;
        Organisation = string.Empty;
        Title = string.Empty;
        Url = null;
        ReviewStatus = string.Empty;
    }

    public GuidelineSource(
        Guid id,
        string referenceCode,
        string organisation,
        string title,
        string? url,
        DateOnly publishedDate,
        DateOnly? lastUpdatedDate,
        DateOnly? lastReviewedDate,
        string reviewStatus)
    {
        Id = id;
        ReferenceCode = Required(referenceCode, nameof(referenceCode));
        Organisation = Required(organisation, nameof(organisation));
        Title = Required(title, nameof(title));
        Url = string.IsNullOrWhiteSpace(url) ? null : url.Trim();
        PublishedDate = publishedDate;
        LastUpdatedDate = lastUpdatedDate;
        LastReviewedDate = lastReviewedDate;
        ReviewStatus = Required(reviewStatus, nameof(reviewStatus));
    }

    public Guid Id { get; private set; }
    public string ReferenceCode { get; private set; }
    public string Organisation { get; private set; }
    public string Title { get; private set; }
    public string? Url { get; private set; }
    public DateOnly PublishedDate { get; private set; }
    public DateOnly? LastUpdatedDate { get; private set; }
    public DateOnly? LastReviewedDate { get; private set; }
    public string ReviewStatus { get; private set; }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{name} is required.", name)
            : value.Trim();
}
