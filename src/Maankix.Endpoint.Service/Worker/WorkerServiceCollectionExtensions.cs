using Maankix.Endpoint.Service.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Maankix.Endpoint.Service.Worker;

/// <summary>
/// Registers the hosted Windows Service worker.
/// </summary>
public static class WorkerServiceCollectionExtensions
{
    /// <summary>
    /// Adds the endpoint hosted worker and its options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddEndpointWorker(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<EndpointServiceOptions>()
            .Bind(configuration.GetSection(EndpointServiceOptions.SectionName))
            .Validate(
                static options => !string.IsNullOrWhiteSpace(options.ServiceName),
                "Maankix:Endpoint:ServiceName is required.")
            .Validate(
                static options => options.ShutdownTimeout > TimeSpan.Zero,
                "Maankix:Endpoint:ShutdownTimeout must be greater than zero.")
            .ValidateOnStart();

        services.AddHostedService<EndpointWorker>();

        return services;
    }
}
