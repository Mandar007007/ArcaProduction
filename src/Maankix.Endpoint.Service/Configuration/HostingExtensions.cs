using Maankix.Endpoint.Service.Common;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Maankix.Endpoint.Service.Configuration;

/// <summary>
/// Composes logging, options, dependency injection, and Windows Service lifetime.
/// </summary>
public static class HostingExtensions
{
    /// <summary>
    /// Configures the Maankix endpoint host. Call from the composition root only.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The same builder.</returns>
    public static HostApplicationBuilder ConfigureMaankixEndpoint(this HostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Logging.ConfigureMaankixLogging(builder.Environment);
        builder.Services.AddMaankixEndpoint(builder.Configuration);
        builder.Services.AddWindowsService(options =>
        {
            options.ServiceName = ServiceIdentity.ServiceName;
        });

        EndpointServiceOptions endpointOptions = builder.Configuration
            .GetSection(EndpointServiceOptions.SectionName)
            .Get<EndpointServiceOptions>()
            ?? new EndpointServiceOptions();

        builder.Services.Configure<HostOptions>(options =>
        {
            options.ShutdownTimeout = endpointOptions.ShutdownTimeout;
        });

        return builder;
    }
}
