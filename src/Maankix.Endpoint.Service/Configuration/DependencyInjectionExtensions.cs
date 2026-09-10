using Maankix.Endpoint.Service.Infrastructure;
using Maankix.Endpoint.Service.Worker;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Maankix.Endpoint.Service.Configuration;

/// <summary>
/// Composition-root service registration for the endpoint host.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds Domain, Application, Infrastructure, and Worker registrations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The same service collection.</returns>
    /// <remarks>
    /// Domain and Application currently expose ports only. Their extension
    /// methods remain in this composition type so those layers stay free of
    /// Microsoft.Extensions.DependencyInjection.
    /// </remarks>
    public static IServiceCollection AddMaankixEndpoint(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        AddDomain(services);
        AddApplication(services);
        services.AddInfrastructure();
        services.AddEndpointWorker(configuration);

        return services;
    }

    /// <summary>
    /// Reserved registration point for Domain services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    private static void AddDomain(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
    }

    /// <summary>
    /// Reserved registration point for Application services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    private static void AddApplication(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
    }
}
