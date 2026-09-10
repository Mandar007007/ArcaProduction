using Microsoft.Extensions.Logging;

namespace Maankix.Endpoint.Service.Worker;

/// <summary>
/// Source-generated log messages for <see cref="EndpointWorker"/>.
/// </summary>
internal static partial class EndpointWorkerLog
{
    [LoggerMessage(
        EventId = 1000,
        EventName = nameof(Started),
        Level = LogLevel.Information,
        Message = "Maankix endpoint worker started. ServiceName={ServiceName}")]
    public static partial void Started(ILogger logger, string serviceName);

    [LoggerMessage(
        EventId = 1001,
        EventName = nameof(Stopping),
        Level = LogLevel.Information,
        Message = "Maankix endpoint worker stopping.")]
    public static partial void Stopping(ILogger logger);
}
