using Maankix.Endpoint.Service.Configuration;

var builder = Host.CreateApplicationBuilder(HostingDefaults.CreateBuilderSettings(args));

builder.ConfigureMaankixEndpoint();

await builder.Build().RunAsync();
