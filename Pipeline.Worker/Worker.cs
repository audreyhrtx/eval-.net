using Pipeline;

namespace Pipeline.Worker;

public sealed class Worker(
    ILogger<Worker> logger,
    IConfiguration configuration,
    PipelineProvider pipelineProvider,
    IPipelineScheduler scheduler,
    IHostApplicationLifetime applicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var url = configuration["PipelineUrl"];
        if (string.IsNullOrWhiteSpace(url))
        {
            url = Environment.GetEnvironmentVariable("PIPELINE_URL");
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            logger.LogError("Aucune URL de pipeline n'est configuree. Utilisez PipelineUrl ou PIPELINE_URL.");
            applicationLifetime.StopApplication();
            return;
        }

        try
        {
            var pipeline = await pipelineProvider.GetPipelineAsync(url, stoppingToken);
            var schedule = scheduler.GetSchedule(pipeline);

            logger.LogInformation("Planning du pipeline :");
            for (var index = 0; index < schedule.Count; index++)
            {
                logger.LogInformation("Groupe {GroupNumber} : {Jobs}", index + 1, string.Join(", ", schedule[index]));
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Le worker a ete arrete.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Impossible de recuperer ou d'ordonnancer le pipeline.");
        }
        finally
        {
            applicationLifetime.StopApplication();
        }
    }
}
