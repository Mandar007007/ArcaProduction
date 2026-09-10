namespace Maankix.Endpoint.Service.Domain;

/// <summary>
/// Port between the user-mode endpoint service and the Windows kernel minifilter.
/// </summary>
/// <remarks>
/// This checkpoint defines the architectural boundary only. Filter-port connection,
/// message schemas, and I/O are intentionally absent.
/// </remarks>
public interface IFilterCommunicationPort
{
}
