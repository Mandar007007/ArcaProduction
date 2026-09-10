using System.Runtime.Versioning;

using Maankix.Endpoint.Service.Application;
using Maankix.Endpoint.Service.Configuration;
using Maankix.Endpoint.Service.Domain;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Maankix.Endpoint.Service.Infrastructure.DriverClient;

/// <summary>
/// MiniSpy user-mode client: <c>FilterConnectCommunicationPort</c>,
/// <c>FilterSendMessage(GetMiniSpyLog)</c>, parse packed <c>LOG_RECORD</c>s.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class MiniSpyDriverClient : IDriverClient
{
    private readonly ILogger<MiniSpyDriverClient> _logger;
    private readonly DriverClientOptions _options;
    private readonly List<string> _attachedVolumes = [];
    private nint _port;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MiniSpyDriverClient"/> class.
    /// </summary>
    /// <param name="logger">Structured logger.</param>
    /// <param name="options">Bound driver client options.</param>
    public MiniSpyDriverClient(ILogger<MiniSpyDriverClient> logger, IOptions<DriverClientOptions> options)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);

        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc />
    public event EventHandler<FileIoObservedEventArgs>? FileIoObserved;

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (MiniSpyProtocol.LogRecordHeaderSize < 152 || MiniSpyProtocol.CommandMessageSize < 8)
        {
            DriverClientLog.ProtocolLayoutMismatch(
                _logger,
                MiniSpyProtocol.LogRecordHeaderSize,
                MiniSpyProtocol.CommandMessageSize);
            throw new InvalidOperationException("MiniSpy LOG_RECORD layout does not match the expected 64-bit MiniSpy protocol.");
        }

        byte[] receiveBuffer = new byte[MiniSpyProtocol.BufferSize];

        while (!cancellationToken.IsCancellationRequested)
        {
            if (!await ConnectWithRetryAsync(cancellationToken).ConfigureAwait(false))
            {
                return;
            }

            AttachConfiguredVolumes();

            try
            {
                await PumpRecordsAsync(receiveBuffer, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            finally
            {
                ClosePort();
            }
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return ValueTask.CompletedTask;
        }

        DetachConfiguredVolumes();
        ClosePort();
        _disposed = true;
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    private async Task<bool> ConnectWithRetryAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            DriverClientLog.Connecting(_logger, _options.PortName);

            int hr = FltLibNative.FilterConnectCommunicationPort(
                _options.PortName,
                0,
                nint.Zero,
                0,
                nint.Zero,
                out nint port);

            if (hr >= 0 && port != nint.Zero && port != -1)
            {
                _port = port;
                DriverClientLog.Connected(_logger, _options.PortName);
                return true;
            }

            DriverClientLog.ConnectFailed(_logger, _options.PortName, hr);

            try
            {
                await Task.Delay(_options.ConnectRetryMilliseconds, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return false;
            }
        }

        return false;
    }

    private async Task PumpRecordsAsync(byte[] receiveBuffer, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            CommandMessage command = new()
            {
                Command = MiniSpyProtocol.GetMiniSpyLog,
                Reserved = 0
            };

            int hr = FltLibNative.FilterSendMessage(
                _port,
                ref command,
                (uint)MiniSpyProtocol.CommandMessageSize,
                receiveBuffer,
                (uint)receiveBuffer.Length,
                out uint bytesReturned);

            if (hr < 0)
            {
                if (hr == MiniSpyProtocol.HresultInvalidHandle)
                {
                    DriverClientLog.FilterUnloaded(_logger);
                    return;
                }

                if (hr != MiniSpyProtocol.HresultNoMoreItems && hr != MiniSpyProtocol.NtStatusNoMoreEntries)
                {
                    DriverClientLog.UnexpectedSendError(_logger, hr);
                }

                await Task.Delay(_options.PollIntervalMilliseconds, cancellationToken).ConfigureAwait(false);
                continue;
            }

            if (bytesReturned == 0)
            {
                await Task.Delay(_options.PollIntervalMilliseconds, cancellationToken).ConfigureAwait(false);
                continue;
            }

            foreach (FileIoObservedEventArgs observed in MiniSpyRecordParser.Parse(
                receiveBuffer.AsSpan(0, (int)bytesReturned)))
            {
                FileIoObserved?.Invoke(this, observed);
            }
        }
    }

    private void AttachConfiguredVolumes()
    {
        foreach (string volume in _options.AttachVolumes)
        {
            if (string.IsNullOrWhiteSpace(volume))
            {
                continue;
            }

            char[] instanceName = new char[256];
            int hr = FltLibNative.FilterAttach(
                _options.FilterName,
                volume,
                null,
                (uint)(instanceName.Length * sizeof(char)),
                instanceName);

            if (hr >= 0)
            {
                _attachedVolumes.Add(volume);
                DriverClientLog.Attached(
                    _logger,
                    _options.FilterName,
                    volume,
                    instanceName.AsSpan().TrimEnd('\0').ToString());
            }
            else
            {
                DriverClientLog.AttachFailed(_logger, _options.FilterName, volume, hr);
            }
        }
    }

    private void DetachConfiguredVolumes()
    {
        foreach (string volume in _attachedVolumes)
        {
            _ = FltLibNative.FilterDetach(_options.FilterName, volume, null);
        }

        _attachedVolumes.Clear();
    }

    private void ClosePort()
    {
        if (_port == nint.Zero || _port == -1)
        {
            return;
        }

        _ = FltLibNative.CloseHandle(_port);
        _port = nint.Zero;
        DriverClientLog.Disconnected(_logger);
    }
}
