using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.Host;
using ModularPipelines.Modules;

await PipelineHostBuilder.Create()
	.ExecutePipelineAsync();

/*
 * steps:
 * start infisical
 * download infisical API spec
 * stop infisical
 * nswag generation
 * nswag correction
 * build
 * pack
 * upload
*/