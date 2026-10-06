using ClarityClinical.Application.Demo;

namespace ClarityClinical.Api.Background;

public sealed class DemoSessionCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<DemoSessionCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var sessions = scope.ServiceProvider.GetRequiredService<IDemoSessionService>();
                var removed = await sessions.DeleteExpiredAsync(
                    DateTimeOffset.UtcNow,
                    stoppingToken);
                if (removed > 0)
                {
                    logger.LogInformation(
                        "Removed {ExpiredDemoSessionCount} expired demo sessions.",
                        removed);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Demo session cleanup failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
