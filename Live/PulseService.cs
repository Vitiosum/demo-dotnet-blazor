namespace cc_dotnet_demo.Live;

/// <summary>Battement côté serveur : toutes les PULSE_MS, l'état de la salle est poussé à chaque navigateur.</summary>
public sealed class PulseService(LiveRoom room) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(DemoSettings.PulseMs));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            room.Tick();
        }
    }
}
