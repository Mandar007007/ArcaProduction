using Maankix.Endpoint.Service.Application;
using Maankix.Endpoint.Service.Configuration;
using Maankix.Endpoint.Service.Domain;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Maankix.Endpoint.Service.Worker;

/// <summary>
/// Hosted worker that pumps minifilter records and logs file I/O events.
/// </summary>
/// <remarks>
/// Observes only. No policy evaluation, blocking, or backend communication.
/// </remarks>
public sealed class EndpointWorker : BackgroundService
{
    private readonly ILogger<EndpointWorker> _logger;
    private readonly EndpointServiceOptions _options;
    private readonly IDriverClient _driverClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointWorker"/> class.
    /// </summary>
    /// <param name="logger">Structured logger for worker lifetime events.</param>
    /// <param name="options">Bound endpoint service options.</param>
    /// <param name="driverClient">MiniSpy protocol client.</param>
    public EndpointWorker(
        ILogger<EndpointWorker> logger,
        IOptions<EndpointServiceOptions> options,
        IDriverClient driverClient)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(driverClient);

        _logger = logger;
        _options = options.Value;
        _driverClient = driverClient;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        EndpointWorkerLog.Started(_logger, _options.ServiceName);
        _driverClient.FileIoObserved += OnFileIoObserved;

        try
        {
            await _driverClient.RunAsync(stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        finally
        {
            _driverClient.FileIoObserved -= OnFileIoObserved;
            EndpointWorkerLog.Stopping(_logger);
        }
    }

    private void OnFileIoObserved(object? sender, FileIoObservedEventArgs e)
    {
        switch (e.Kind)
        {
            case FileIoKind.Create:
                EndpointWorkerLog.FileCreate(_logger, e.FullPath, e.SequenceNumber, e.ProcessId, e.Status);
                break;
            case FileIoKind.Read:
                EndpointWorkerLog.FileRead(_logger, e.FullPath, e.SequenceNumber, e.ProcessId, e.Status);
                break;
            case FileIoKind.Write:
                EndpointWorkerLog.FileWrite(_logger, e.FullPath, e.SequenceNumber, e.ProcessId, e.Status);
                break;
            case FileIoKind.Close:
                EndpointWorkerLog.FileClose(_logger, e.FullPath, e.SequenceNumber, e.ProcessId, e.Status);
                break;
        }
    }
}
