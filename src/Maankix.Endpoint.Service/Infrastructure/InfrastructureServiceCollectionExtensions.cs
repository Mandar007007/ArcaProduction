using Maankix.Endpoint.Service.Application;
using Maankix.Endpoint.Service.Configuration;
using Maankix.Endpoint.Service.Infrastructure.DriverClient;

using Microsoft.Extensions.Configuration;
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
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton(TimeProvider.System);

        services
            .AddOptions<DriverClientOptions>()
            .Bind(configuration.GetSection(DriverClientOptions.SectionName))
            .Validate(
                static options => !string.IsNullOrWhiteSpace(options.PortName),
                "Maankix:Driver:PortName is required.")
            .Validate(
                static options => !string.IsNullOrWhiteSpace(options.FilterName),
                "Maankix:Driver:FilterName is required.")
            .Validate(
                static options => options.PollIntervalMilliseconds > 0,
                "Maankix:Driver:PollIntervalMilliseconds must be greater than zero.")
            .Validate(
                static options => options.ConnectRetryMilliseconds > 0,
                "Maankix:Driver:ConnectRetryMilliseconds must be greater than zero.")
            .ValidateOnStart();

        services.AddSingleton<IDriverClient, MiniSpyDriverClient>();

        return services;
    }
}
