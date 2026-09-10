using Maankix.Endpoint.Service.Configuration;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Maankix.Endpoint.Service.Worker;

/// <summary>
/// Long-running hosted worker that keeps the Windows Service process alive.
/// </summary>
/// <remarks>
/// This checkpoint performs host lifetime work only. No policy evaluation,
/// driver I/O, or backend communication is performed.
/// </remarks>
public sealed class EndpointWorker : BackgroundService
{
    private readonly ILogger<EndpointWorker> _logger;
    private readonly EndpointServiceOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointWorker"/> class.
    /// </summary>
    /// <param name="logger">Structured logger for worker lifetime events.</param>
    /// <param name="options">Bound endpoint service options.</param>
    public EndpointWorker(
        ILogger<EndpointWorker> logger,
        IOptions<EndpointServiceOptions> options)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);

        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        EndpointWorkerLog.Started(_logger, _options.ServiceName);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }

        EndpointWorkerLog.Stopping(_logger);
    }
}
