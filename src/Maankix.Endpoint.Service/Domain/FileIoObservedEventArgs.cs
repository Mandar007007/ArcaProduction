namespace Maankix.Endpoint.Service.Domain;

/// <summary>
/// A file operation parsed from a MiniSpy <c>LOG_RECORD</c>.
/// </summary>
public sealed class FileIoObservedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileIoObservedEventArgs"/> class.
    /// </summary>
    /// <param name="kind">Observed operation.</param>
    /// <param name="fullPath">Path from the filter record (typically an NT device path).</param>
    /// <param name="sequenceNumber">MiniSpy log sequence number.</param>
    /// <param name="processId">Originating process id from the record.</param>
    /// <param name="status">NTSTATUS from the completed operation.</param>
    public FileIoObservedEventArgs(
        FileIoKind kind,
        string fullPath,
        uint sequenceNumber,
        ulong processId,
        int status)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullPath);

        Kind = kind;
        FullPath = fullPath;
        SequenceNumber = sequenceNumber;
        ProcessId = processId;
        Status = status;
    }

    /// <summary>
    /// Gets the observed operation.
    /// </summary>
    public FileIoKind Kind { get; }

    /// <summary>
    /// Gets the file path supplied by the minifilter.
    /// </summary>
    public string FullPath { get; }

    /// <summary>
    /// Gets the MiniSpy sequence number.
    /// </summary>
    public uint SequenceNumber { get; }

    /// <summary>
    /// Gets the process id recorded by the filter.
    /// </summary>
    public ulong ProcessId { get; }

    /// <summary>
    /// Gets the completion NTSTATUS.
    /// </summary>
    public int Status { get; }
}
