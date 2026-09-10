using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;

namespace Maankix.Endpoint.Service.Configuration;

/// <summary>
/// Default <see cref="HostApplicationBuilder"/> settings for the endpoint process.
/// </summary>
public static class HostingDefaults
{
    /// <summary>
    /// Creates host builder settings that keep configuration files resolvable
    /// when Service Control Manager starts the process from System32.
    /// </summary>
    /// <param name="args">Command-line arguments forwarded to the host.</param>
    /// <returns>Settings for <see cref="Host.CreateApplicationBuilder(HostApplicationBuilderSettings)"/>.</returns>
    public static HostApplicationBuilderSettings CreateBuilderSettings(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        return new HostApplicationBuilderSettings
        {
            Args = args,
            ContentRootPath = WindowsServiceHelpers.IsWindowsService()
                ? AppContext.BaseDirectory
                : null
        };
    }
}
