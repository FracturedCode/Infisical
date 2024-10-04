using System.Linq.Expressions;
using EnumerableAsyncProcessor.Extensions;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.FileSystem;
using ModularPipelines.Git.Extensions;
using Net.FracturedCode.Infisical.Pipeline.Generation;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using Module = ModularPipelines.Modules.Module;

namespace Net.FracturedCode.Infisical.Pipeline.Modules;

[DependsOn<DownloadInfisicalSpec>]
public class GenerateInfisicalClients : Module
{
	protected override async Task<IDictionary<string, object>?> ExecuteAsync(IPipelineContext context,
		CancellationToken cancellationToken)
	{
		string? specJson = (await GetModule<DownloadInfisicalSpec>()).Value;
		ArgumentException.ThrowIfNullOrWhiteSpace(specJson);

		OpenApiDocument document = await OpenApiDocument.FromJsonAsync(specJson, cancellationToken);

		Folder sdkFolder = context.Git().RootDirectory.GetFolder("Infisical");

		Folder sdkGeneratedFolder = sdkFolder.GetFolder("Generated").Create();

		string nswagTemplateJson = await sdkFolder
			.GetFile("nswag.template.json")
			.ReadAsync(cancellationToken);

		NswagGenerator generator = new(document, nswagTemplateJson, sdkGeneratedFolder, context.Logger, cancellationToken);

		List<(string, Expression<Func<CSharpClientGeneratorSettings, bool>>)> generations =
		[
			("Clients", s => s.GenerateClientClasses),
			("Dtos", s => s.GenerateDtoTypes),
			("Responses", s => s.GenerateResponseClasses),
			("Exceptions", s => s.GenerateExceptionClasses)
		];

		// TODO SubModule NullReferenceException
		await generations.ForEachAsync(
				async x => await generator.GenerateFile(
					x.Item1,
					x.Item2,
					generations
						.Where(g => g.Item1 != x.Item1)
						.Select(g => g.Item1)),
				cancellationToken)
			.ProcessInParallel()
			.WaitAsync();

		return await NothingAsync();
	}
}