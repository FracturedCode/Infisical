using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
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
public partial class Pack(IOptions<NugetOptions> nugetOptions) : Module<File>
{
	protected override async Task<File?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
	{
		CommandResult packResult = await context.DotNet().Pack(new DotNetPackOptions
		{
			ProjectSolution = context.Git().RootDirectory.GetFiles(f => f.Name == "Infisical.csproj").Single(),
			NoBuild = true,
			Configuration = "Release",
			VersionSuffix = nugetOptions.Value.PushToProd ? null : $"alpha.{context.Git().Information.LastCommitShortSha}"
		}, cancellationToken);
		var packages = packageRegex().Matches(packResult.StandardOutput)
			.Select(f => new File(f.Value))
			.ToList();
		// Ensure snupkg exists
		_ = packages.Single(p => p.Extension == ".snupkg");
		return packages.Single(p => p.Extension == ".nupkg");
	}

	[GeneratedRegex(@"(?<=Successfully created package ').*(?='\.)")]
	private static partial Regex packageRegex();
}