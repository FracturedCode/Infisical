using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Modules;
using File = ModularPipelines.FileSystem.File;

namespace Net.FracturedCode.Infisical.Pipeline.Modules;

[DependsOn<GenerateInfisicalClients>]
public class Build : Module
{
	protected override async Task<IDictionary<string, object>?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
	{
		File solution = context.Git().RootDirectory.GetFile("Infisical.sln");
		await context.DotNet().Restore(new DotNetRestoreOptions { Path = solution }, cancellationToken);
		await context.DotNet().Build(new DotNetBuildOptions { ProjectSolution = solution, Configuration = "Release" }, cancellationToken);
		return await NothingAsync();
	}
}