using ModularPipelines.Host;
using Net.FracturedCode.Infisical.Pipeline;

await PipelineHostBuilder.Create()
	.AddModule<DownloadInfisicalSpec>()
	.ExecutePipelineAsync();

/*
 * steps:
 * start infisical
 * download infisical API spec
 * stop infisical
 * 
 * nswag generation
 * nswag correction
 * restore
 * build
 * test
 * pack
 * upload
*/