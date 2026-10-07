namespace ClarityClinical.Application.Speech;

public sealed record SpeechAuthorization(
    string Token,
    string Region,
    DateTimeOffset RefreshAfter);

public interface ISpeechAuthorizationProvider
{
    Task<SpeechAuthorization> GetAuthorizationAsync(CancellationToken cancellationToken);
}

public sealed class SpeechServiceUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
