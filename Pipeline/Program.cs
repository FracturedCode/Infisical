using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularPipelines.Host;
using Net.FracturedCode.Infisical.Pipeline.Modules;

await PipelineHostBuilder.Create()
	.ConfigureAppConfiguration((_, cb) =>
	{
		cb.AddUserSecrets<Program>().AddEnvironmentVariables("DOTNET_");
	})
	.ConfigureServices((hbc, sc) =>
	{
		sc.Configure<NugetOptions>(hbc.Configuration.GetSection("Nuget"));
	})
	.AddModule<DownloadInfisicalSpec>()
	.AddModule<GenerateInfisicalClients>()
	.AddModule<Build>()
	.AddModule<Test>()
	.AddModule<Pack>()
	.AddModule<Upload>()
	.ExecutePipelineAsync();