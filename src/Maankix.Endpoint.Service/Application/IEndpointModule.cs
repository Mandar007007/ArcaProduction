namespace Maankix.Endpoint.Service.Application;

/// <summary>
/// Application module that can be composed into the endpoint host.
/// </summary>
/// <remarks>
/// Feature modules (policy evaluation, telemetry, and similar) will implement this
/// contract in later checkpoints. The interface is empty so composition can be
/// introduced without inventing behavior now.
/// </remarks>
public interface IEndpointModule
{
}
