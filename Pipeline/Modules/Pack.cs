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
		_ = packages.Single(p => p.Extension == ".snupkg"); // Ensure snupkg exists
		File nupkg = packages.Single(p => p.Extension == ".nupkg");
		
		if (context.BuildSystemDetector.IsRunningOnGitHubActions)
		{
			string ghOutputPath = context.Configuration["GITHUB_OUTPUT"] ?? throw new Exception();
			await System.IO.File.AppendAllTextAsync(ghOutputPath, $"NupkgPath={nupkg.Path}", cancellationToken);
		}
		
		return nupkg;
	}

	[GeneratedRegex(@"(?<=Successfully created package ').*(?='\.)")]
	private static partial Regex packageRegex();
}