namespace Maankix.Endpoint.Service.Domain;

/// <summary>
/// Domain port for file-system events originating in the kernel minifilter.
/// </summary>
public interface IFilterCommunicationPort
{
    /// <summary>
    /// Raised when a CREATE, READ, WRITE, or CLOSE record is parsed.
    /// </summary>
    event EventHandler<FileIoObservedEventArgs>? FileIoObserved;
}
