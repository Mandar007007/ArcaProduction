namespace Maankix.Endpoint.Service.Domain;

/// <summary>
/// File-system operations the endpoint observes from the minifilter.
/// </summary>
public enum FileIoKind
{
    /// <summary>
    /// <c>IRP_MJ_CREATE</c>.
    /// </summary>
    Create,

    /// <summary>
    /// <c>IRP_MJ_READ</c> and MiniSpy MDL-read variants.
    /// </summary>
    Read,

    /// <summary>
    /// <c>IRP_MJ_WRITE</c> and MiniSpy MDL-write variants.
    /// </summary>
    Write,

    /// <summary>
    /// <c>IRP_MJ_CLOSE</c>.
    /// </summary>
    Close
}
