using ClarityClinical.Application.Speech;
using ClarityClinical.Infrastructure.Speech;

namespace ClarityClinical.Api.Configuration;

public static class SpeechExtensions
{
    public static IServiceCollection AddClarityClinicalSpeech(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient("AzureSpeechAuthorization");
        services.AddScoped<ISpeechAuthorizationProvider>(serviceProvider =>
        {
            var client = serviceProvider
                .GetRequiredService<IHttpClientFactory>()
                .CreateClient("AzureSpeechAuthorization");
            return new AzureSpeechAuthorizationProvider(
                client,
                configuration["Speech:SubscriptionKey"],
                configuration["Speech:Region"]);
        });

        return services;
    }
}
