using ClarityClinical.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClarityClinical.IntegrationTests.Persistence;

public sealed class DatabaseSchemaModeTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("false", false)]
    [InlineData("nonsense", false)]
    [InlineData("true", true)]
    [InlineData("True", true)]
    public void Public_schema_only_is_explicit_opt_in(string? value, bool expected) =>
        Assert.Equal(expected, DatabaseSchemaMode.Parse(value));

    [Fact]
    public void Default_mode_keeps_application_schemas()
    {
        var model = Build(publicSchemaOnly: false);
        Assert.Contains(model.GetEntityTypes(), entity => entity.GetSchema() == DbSchemas.Admin);
        Assert.DoesNotContain(model.GetEntityTypes(), entity => entity.GetSchema() is null);
    }

    [Fact]
    public void Public_schema_only_mode_maps_every_table_to_public_without_name_collisions()
    {
        var model = Build(publicSchemaOnly: true);
        Assert.All(model.GetEntityTypes(), entity => Assert.Null(entity.GetSchema()));
        var tables = model.GetEntityTypes().Select(entity => entity.GetTableName()).ToList();
        Assert.Equal(tables.Count, tables.Distinct(StringComparer.Ordinal).Count());
    }

    private static Microsoft.EntityFrameworkCore.Metadata.IMutableModel Build(bool publicSchemaOnly)
    {
        var builder = new ModelBuilder();
        builder.ApplyConfigurationsFromAssembly(typeof(ClarityClinicalDbContext).Assembly);
        DatabaseSchemaMode.Apply(builder, publicSchemaOnly);
        return builder.Model;
    }
}
