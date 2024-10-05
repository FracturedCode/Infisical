using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularPipelines.Host;
using Net.FracturedCode.Infisical.Pipeline.Modules;
using Net.FracturedCode.Infisical.Pipeline;

// TODO gh attestations?

await PipelineHostBuilder.Create()
	.ConfigureAppConfiguration((_, cb) =>
	{
		cb.AddUserSecrets<Program>().AddEnvironmentVariables("DOTNET_");
	})
	.ConfigureServices((hbc, sc) =>
	{
		sc.Configure<NugetOptions>(hbc.Configuration.GetSection("Nuget"));
	})
	.AddAllModules()
	.ExecutePipelineAsync();