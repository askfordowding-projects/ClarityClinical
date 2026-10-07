using System.Net.Http.Headers;
using ClarityClinical.Application.Speech;

namespace ClarityClinical.Infrastructure.Speech;

public sealed class AzureSpeechAuthorizationProvider(
    HttpClient httpClient,
    string? subscriptionKey,
    string? region) : ISpeechAuthorizationProvider
{
    public async Task<SpeechAuthorization> GetAuthorizationAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(subscriptionKey) || string.IsNullOrWhiteSpace(region))
        {
            throw new SpeechServiceUnavailableException(
                "Speech translation is not configured for this environment.");
        }

        var normalizedRegion = region.Trim();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://{normalizedRegion}.api.cognitive.microsoft.com/sts/v1.0/issueToken");
        request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey.Trim());
        request.Content = new ByteArrayContent([]);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new SpeechServiceUnavailableException(
                    $"Speech authorization service returned HTTP {(int)response.StatusCode}.");
            }

            var token = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new SpeechServiceUnavailableException(
                    "Speech authorization service returned an empty token.");
            }

            return new SpeechAuthorization(
                token,
                normalizedRegion,
                DateTimeOffset.UtcNow.AddMinutes(9));
        }
        catch (SpeechServiceUnavailableException)
        {
            throw;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            throw new SpeechServiceUnavailableException(
                "Speech authorization service could not be reached.",
                exception);
        }
    }
}
