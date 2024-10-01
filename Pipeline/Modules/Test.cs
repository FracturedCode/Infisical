using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Modules;

namespace Net.FracturedCode.Infisical.Pipeline.Modules;

[DependsOn<Build>]
public class Test : Module
{
	protected override async Task<IDictionary<string, object>?> ExecuteAsync(IPipelineContext context,
		CancellationToken cancellationToken)
	{
		await context.DotNet().Test(
			new DotNetTestOptions
			{
				ProjectSolutionDirectoryDllExe = context.Git().RootDirectory.GetFile("Tests/Tests.csproj"),
				Configuration = "Release",
				NoBuild = true
			},
			cancellationToken);
		return await NothingAsync();
	}
}