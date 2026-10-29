using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ClarityClinical.Infrastructure.Persistence;

/// <summary>
/// Some managed PostgreSQL providers (OVH Web Cloud Databases) do not allow
/// application accounts to create schemas. When
/// CLARITYCLINICAL_DB_PUBLIC_SCHEMA_ONLY=true every table is mapped to the
/// public schema instead of its own schema. The deployment platform renders the
/// packaged migrations into public for those environments
/// (migrationSchemaMode=public). The default (unset/false) keeps schemas.
/// </summary>
public static class DatabaseSchemaMode
{
    public const string EnvironmentVariable = "CLARITYCLINICAL_DB_PUBLIC_SCHEMA_ONLY";

    public static bool PublicSchemaOnly =>
        Parse(Environment.GetEnvironmentVariable(EnvironmentVariable));

    public static bool Parse(string? value) =>
        bool.TryParse(value, out var publicOnly) && publicOnly;

    public static void Apply(ModelBuilder modelBuilder, bool publicSchemaOnly)
    {
        if (!publicSchemaOnly)
        {
            return;
        }

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            entityType.SetSchema(null);
        }
    }
}

/// <summary>
/// Keys the EF model cache by schema mode so a model built for one mode is never
/// reused for the other within the same process.
/// </summary>
public sealed class SchemaModeModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime) =>
        (context.GetType(), DatabaseSchemaMode.PublicSchemaOnly, designTime);
}
