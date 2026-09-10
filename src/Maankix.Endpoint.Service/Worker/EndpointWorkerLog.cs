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

    [LoggerMessage(
        EventId = 1100,
        EventName = nameof(FileCreate),
        Level = LogLevel.Information,
        Message = "FILE_CREATE Path={Path} Sequence={Sequence} ProcessId={ProcessId} Status=0x{Status:X8}")]
    public static partial void FileCreate(ILogger logger, string path, uint sequence, ulong processId, int status);

    [LoggerMessage(
        EventId = 1101,
        EventName = nameof(FileRead),
        Level = LogLevel.Information,
        Message = "FILE_READ Path={Path} Sequence={Sequence} ProcessId={ProcessId} Status=0x{Status:X8}")]
    public static partial void FileRead(ILogger logger, string path, uint sequence, ulong processId, int status);

    [LoggerMessage(
        EventId = 1102,
        EventName = nameof(FileWrite),
        Level = LogLevel.Information,
        Message = "FILE_WRITE Path={Path} Sequence={Sequence} ProcessId={ProcessId} Status=0x{Status:X8}")]
    public static partial void FileWrite(ILogger logger, string path, uint sequence, ulong processId, int status);

    [LoggerMessage(
        EventId = 1103,
        EventName = nameof(FileClose),
        Level = LogLevel.Information,
        Message = "FILE_CLOSE Path={Path} Sequence={Sequence} ProcessId={ProcessId} Status=0x{Status:X8}")]
    public static partial void FileClose(ILogger logger, string path, uint sequence, ulong processId, int status);
}
