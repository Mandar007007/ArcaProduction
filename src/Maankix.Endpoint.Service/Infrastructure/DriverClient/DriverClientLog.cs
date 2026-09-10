using Microsoft.Extensions.Logging;

namespace Maankix.Endpoint.Service.Infrastructure.DriverClient;

/// <summary>
/// Source-generated log messages for <see cref="MiniSpyDriverClient"/>.
/// </summary>
internal static partial class DriverClientLog
{
    [LoggerMessage(
        EventId = 2000,
        EventName = nameof(Connecting),
        Level = LogLevel.Information,
        Message = "Connecting to minifilter port {PortName}.")]
    public static partial void Connecting(ILogger logger, string portName);

    [LoggerMessage(
        EventId = 2001,
        EventName = nameof(Connected),
        Level = LogLevel.Information,
        Message = "Connected to minifilter port {PortName}.")]
    public static partial void Connected(ILogger logger, string portName);

    [LoggerMessage(
        EventId = 2002,
        EventName = nameof(ConnectFailed),
        Level = LogLevel.Warning,
        Message = "Could not connect to minifilter port {PortName}. HRESULT=0x{HResult:X8}. Retrying.")]
    public static partial void ConnectFailed(ILogger logger, string portName, int hResult);

    [LoggerMessage(
        EventId = 2003,
        EventName = nameof(Attached),
        Level = LogLevel.Information,
        Message = "Attached filter {FilterName} to {Volume}. Instance={Instance}.")]
    public static partial void Attached(ILogger logger, string filterName, string volume, string instance);

    [LoggerMessage(
        EventId = 2004,
        EventName = nameof(AttachFailed),
        Level = LogLevel.Warning,
        Message = "Could not attach filter {FilterName} to {Volume}. HRESULT=0x{HResult:X8}.")]
    public static partial void AttachFailed(ILogger logger, string filterName, string volume, int hResult);

    [LoggerMessage(
        EventId = 2005,
        EventName = nameof(FilterUnloaded),
        Level = LogLevel.Warning,
        Message = "Minifilter port handle is invalid; the kernel component may have unloaded.")]
    public static partial void FilterUnloaded(ILogger logger);

    [LoggerMessage(
        EventId = 2006,
        EventName = nameof(UnexpectedSendError),
        Level = LogLevel.Warning,
        Message = "Unexpected FilterSendMessage HRESULT=0x{HResult:X8}.")]
    public static partial void UnexpectedSendError(ILogger logger, int hResult);

    [LoggerMessage(
        EventId = 2007,
        EventName = nameof(Disconnected),
        Level = LogLevel.Information,
        Message = "Disconnected from minifilter port.")]
    public static partial void Disconnected(ILogger logger);

    [LoggerMessage(
        EventId = 2008,
        EventName = nameof(ProtocolLayoutMismatch),
        Level = LogLevel.Error,
        Message = "MiniSpy LOG_RECORD layout mismatch. HeaderSize={HeaderSize} CommandSize={CommandSize}.")]
    public static partial void ProtocolLayoutMismatch(ILogger logger, int headerSize, int commandSize);
}
