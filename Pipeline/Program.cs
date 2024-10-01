using ModularPipelines.Host;
using Net.FracturedCode.Infisical.Pipeline.Modules;

await PipelineHostBuilder.Create()
	.AddModule<DownloadInfisicalSpec>()
	.AddModule<GenerateInfisicalClients>()
	.AddModule<Build>()
	.ExecutePipelineAsync();

/*
 * steps:
 * start infisical
 * download infisical API spec
 * stop infisical
 * nswag generation
 * nswag correction
 * restore
 * build
 * 
 * test
 * pack
 * upload
*/