using Maankix.Endpoint.Service.Common;

namespace Maankix.Endpoint.Service.Configuration;

/// <summary>
/// Configuration for the endpoint Windows Service host.
/// </summary>
public sealed class EndpointServiceOptions
{
    /// <summary>
    /// Configuration section path bound to this type.
    /// </summary>
    public const string SectionName = "Maankix:Endpoint";

    /// <summary>
    /// Logical service name used in structured logs. Defaults to
    /// <see cref="ServiceIdentity.ServiceName"/>.
    /// </summary>
    public string ServiceName { get; set; } = ServiceIdentity.ServiceName;

    /// <summary>
    /// Time the generic host waits for hosted services to stop.
    /// </summary>
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
