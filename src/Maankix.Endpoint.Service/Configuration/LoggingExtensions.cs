using Maankix.Endpoint.Service.Common;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

namespace Maankix.Endpoint.Service.Configuration;

/// <summary>
/// Structured logging providers for the endpoint host.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configures console logging, debug output, and the Windows Event Log when hosted by SCM.
    /// </summary>
    /// <param name="logging">The logging builder.</param>
    /// <param name="environment">The current host environment.</param>
    /// <returns>The same logging builder.</returns>
    public static ILoggingBuilder ConfigureMaankixLogging(
        this ILoggingBuilder logging,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(logging);
        ArgumentNullException.ThrowIfNull(environment);

        logging.ClearProviders();

        if (environment.IsDevelopment())
        {
            logging.AddSimpleConsole();
        }
        else
        {
            logging.AddJsonConsole();
        }

        logging.AddDebug();

        if (WindowsServiceHelpers.IsWindowsService())
        {
            logging.AddEventLog(new EventLogSettings
            {
                SourceName = ServiceIdentity.DisplayName,
                LogName = "Application"
            });
        }

        return logging;
    }
}
