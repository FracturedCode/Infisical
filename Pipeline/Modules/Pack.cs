using System.Text.RegularExpressions;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Models;
using ModularPipelines.Modules;
using File = ModularPipelines.FileSystem.File;

namespace Net.FracturedCode.Infisical.Pipeline.Modules;

[DependsOn<Build>]
public partial class Pack : Module<File>
{
	protected override async Task<File?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
	{
		CommandResult packResult = await context.DotNet().Pack(new DotNetPackOptions
			{
				ProjectSolution = context.Git().RootDirectory.GetFiles(f => f.Name == "Infisical.csproj").Single(),
				NoBuild = true,
				Configuration = "Release"
			},
			cancellationToken);
		return packageRegex().Matches(packResult.StandardOutput)
			.Select(f => new File(f.Value))
			.Single();
	}

	[GeneratedRegex(@"(?<=Successfully created package ').*(?='\.)")]
	private static partial Regex packageRegex();
}