using ClarityClinical.Domain.Governance;
using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClarityClinical.Infrastructure.Admin;

public sealed class GovernanceSeeder(ClarityClinicalDbContext dbContext)
{
    private static readonly Guid DvtSourceId = Guid.Parse("01c058ce-527b-43df-86ef-6dd3c3734617");
    private static readonly Guid CellulitisSourceId = Guid.Parse("e2406cf6-d3fe-4ae7-bfdb-625e6574384d");
    private static readonly Guid MskSourceId = Guid.Parse("97dedad8-77ae-4661-9f04-14c149f5bcdb");

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.GuidelineSources.AnyAsync(cancellationToken)) return;

        var dvt = new GuidelineSource(DvtSourceId, "NG158", "NICE", "Venous thromboembolic diseases: diagnosis, management and thrombophilia testing", "https://www.nice.org.uk/guidance/ng158", new DateOnly(2020, 3, 26), null, null, "ReviewRequired");
        var cellulitis = new GuidelineSource(CellulitisSourceId, "NG141", "NICE", "Cellulitis and erysipelas: antimicrobial prescribing", "https://www.nice.org.uk/guidance/ng141", new DateOnly(2019, 9, 27), null, null, "ReviewRequired");
        var msk = new GuidelineSource(MskSourceId, "DEMO-MSK", "Clarity Clinical", "Illustrative musculoskeletal demonstration rule", null, new DateOnly(2026, 10, 8), null, new DateOnly(2026, 10, 8), "DemonstrationOnly");
        dbContext.GuidelineSources.AddRange(dvt, cellulitis, msk);

        const string provenance = "Illustrative demo logic only; this score is not a validated disease probability or clinical risk score.";
        dbContext.ClinicalRuleVersions.AddRange(
            new ClinicalRuleVersion(Guid.Parse("36be2cce-58f5-4a62-b830-c19827c80e95"), "dvt", "Deep vein thrombosis", "demo-illustrative-1.0", "Active", "IllustrativePriority", true, provenance, DvtSourceId, new DateOnly(2026, 10, 8)),
            new ClinicalRuleVersion(Guid.Parse("d4a18891-c145-4ddb-a854-f73d57110902"), "cellulitis", "Cellulitis", "demo-illustrative-1.0", "Active", "IllustrativePriority", true, provenance, CellulitisSourceId, new DateOnly(2026, 10, 8)),
            new ClinicalRuleVersion(Guid.Parse("579444ca-619f-42a6-883b-19becb60e975"), "musculoskeletal-inflammation", "Musculoskeletal inflammation", "demo-illustrative-1.0", "Active", "IllustrativePriority", true, provenance, MskSourceId, new DateOnly(2026, 10, 8)));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
