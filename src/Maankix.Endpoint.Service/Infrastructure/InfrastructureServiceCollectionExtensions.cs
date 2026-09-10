using Microsoft.Extensions.DependencyInjection;

namespace Maankix.Endpoint.Service.Infrastructure;

/// <summary>
/// Registers Infrastructure services. Domain types may be used here; Domain must not
/// take a dependency on this assembly layer.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Adds Infrastructure services for the endpoint host.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
