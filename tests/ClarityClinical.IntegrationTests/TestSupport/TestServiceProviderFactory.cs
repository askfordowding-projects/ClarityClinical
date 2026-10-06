using ClarityClinical.Api.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClarityClinical.IntegrationTests.TestSupport;

public static class TestServiceProviderFactory
{
    public static ServiceProvider Create(string connectionString)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:ClarityClinical"] = connectionString
            })
            .Build();

        var services = new ServiceCollection();
        services.AddClarityClinicalPersistence(configuration);
        return services.BuildServiceProvider();
    }
}
