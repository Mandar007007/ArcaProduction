using Maankix.Endpoint.Service.Domain;

namespace Maankix.Endpoint.Service.Application;

/// <summary>
/// User-mode client that replaces MiniSpy.exe: connect, poll, parse, raise events.
/// </summary>
public interface IDriverClient : IFilterCommunicationPort, IAsyncDisposable
{
    /// <summary>
    /// Connects to the filter port and pumps log records until cancelled.
    /// </summary>
    /// <param name="cancellationToken">Stops the receive loop.</param>
    /// <returns>A task that completes when the loop ends.</returns>
    Task RunAsync(CancellationToken cancellationToken);
}
