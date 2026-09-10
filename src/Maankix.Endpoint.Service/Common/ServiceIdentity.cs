namespace Maankix.Endpoint.Service.Common;

/// <summary>
/// Stable identifiers used by Service Control Manager, Event Log, and process diagnostics.
/// </summary>
/// <remarks>
/// These values are the installed service identity. Configuration may echo them for
/// logging but must not be treated as a rename of an already-installed Windows Service.
/// </remarks>
public static class ServiceIdentity
{
    /// <summary>
    /// Short service name registered with Service Control Manager.
    /// </summary>
    public const string ServiceName = "MaankixEndpoint";

    /// <summary>
    /// Display name shown in the Windows Services MMC.
    /// </summary>
    public const string DisplayName = "Maankix Endpoint Service";

    /// <summary>
    /// Description registered with Service Control Manager.
    /// </summary>
    public const string Description = "Maankix Windows endpoint data-loss prevention host.";
}
