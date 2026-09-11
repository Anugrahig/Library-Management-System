using LibraryManagement.Application.Interfaces.Services;

namespace LibraryManagement.API.Workers;

public sealed class ReservationExpirationWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<ReservationExpirationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var reservations = scope.ServiceProvider.GetRequiredService<IReservationService>();
                var expiredCount = await reservations.ExpirePendingAsync(DateTime.UtcNow, stoppingToken);

                if (expiredCount > 0)
                {
                    logger.LogInformation("Expired {Count} pending reservations.", expiredCount);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Reservation expiration failed.");
            }
        }
    }
}
