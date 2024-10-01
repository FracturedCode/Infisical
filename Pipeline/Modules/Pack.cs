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
public partial class Pack : Module<PackResult>
{
	protected override async Task<PackResult?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
	{
		CommandResult packResult = await context.DotNet().Pack(new DotNetPackOptions
			{
				ProjectSolution = context.Git().RootDirectory.GetFiles(f => f.Name == "Infisical.csproj").Single(),
				NoBuild = true,
				IncludeSource = true,
				IncludeSymbols = true,
				Configuration = "Release"
			},
			cancellationToken);
		var nupkgs = packageRegex().Matches(packResult.StandardOutput)
			.Select(f => new File(f.Value))
			.ToList();
		return new PackResult(nupkgs.Single(f => !isSymbolPkg(f)), nupkgs.Single(isSymbolPkg));
		
		bool isSymbolPkg(File f) => f.Name.EndsWith(".symbols.nupkg");
	}

	[GeneratedRegex(@"(?<=Successfully created package ').*(?='\.)")]
	private static partial Regex packageRegex();
}

public record PackResult(File Nupkg, File Snupkg);