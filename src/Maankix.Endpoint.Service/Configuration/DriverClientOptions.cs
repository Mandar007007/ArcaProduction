namespace Maankix.Endpoint.Service.Configuration;

/// <summary>
/// Options for the MiniSpy filter communication client.
/// </summary>
public sealed class DriverClientOptions
{
    /// <summary>
    /// Configuration section path bound to this type.
    /// </summary>
    public const string SectionName = "Maankix:Driver";

    /// <summary>
    /// Filter communication port. Must match <c>MINISPY_PORT_NAME</c>.
    /// </summary>
    public string PortName { get; set; } = @"\MiniSpyPort";

    /// <summary>
    /// Filter Manager name (INF <c>ServiceName</c>).
    /// </summary>
    public string FilterName { get; set; } = "maankixflt";

    /// <summary>
    /// Volumes to attach after connect. MiniSpy instances do not auto-attach.
    /// </summary>
    public string[] AttachVolumes { get; set; } = ["C:\\"];

    /// <summary>
    /// Poll interval when the filter has no records (<c>ERROR_NO_MORE_ITEMS</c>).
    /// MiniSpy uses 200 ms.
    /// </summary>
    public int PollIntervalMilliseconds { get; set; } = 200;

    /// <summary>
    /// Delay between connect retries when the driver is not loaded.
    /// </summary>
    public int ConnectRetryMilliseconds { get; set; } = 2000;
}
