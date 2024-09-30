using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModularPipelines.FileSystem;
using NSwag;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.CSharp;

namespace Net.FracturedCode.Infisical.Pipeline.Generation;

internal class NswagGenerator(
	OpenApiDocument document,
	string nswagTemplateJson,
	Folder sdkProjectFolder,
	ILogger logger,
	CancellationToken cancellationToken)
{
	private static readonly JsonSerializerOptions _serializerOptions = new()
	{
		MaxDepth = 3
	};
	public async Task GenerateFile(string generationName, Expression<Func<CSharpClientGeneratorSettings, bool>> generationTargetExpression)
	{
		// TODO configure in C#
		CSharpClientGeneratorSettings settings =
			JsonSerializer.Deserialize<CSharpClientGeneratorSettings>(nswagTemplateJson, _serializerOptions)
			?? throw new Exception("Could not read nswag template or template was null");
		var prop = (PropertyInfo)((MemberExpression)generationTargetExpression.Body).Member;
		prop.SetValue(settings, true);
		//settings.CodeGeneratorSettings.TypeNameGenerator = new InfisicalTypeNameGenerator();
		settings.OperationNameGenerator = new InfisicalOperationNameGenerator();
		settings.CSharpGeneratorSettings.Namespace = "Net.FracturedCode.Infisical";
		
		CSharpClientGenerator generator = new(document, settings);

		// Generate
		// TODO split further??
		string generation = await Task.Run(
			() =>
			{
				logger.LogInformation("Generating {generationName}", generationName);
				return generator.GenerateFile(ClientGeneratorOutputType.Full);
			},
			cancellationToken);
		cancellationToken.ThrowIfCancellationRequested();

		// Apply corrections
		logger.LogInformation("Correcting {file}", generationName);
		generation = generation.Replace("@$glob", "glob");
		cancellationToken.ThrowIfCancellationRequested();

		// Save
		await sdkProjectFolder.GetFile($"{generationName}.cs").WriteAsync(generation, cancellationToken);
	}
}

/*internal class InfisicalTypeNameGenerator : ITypeNameGenerator
{
	public string Generate(JsonSchema schema, string? typeNameHint, IEnumerable<string> reservedTypeNames)
	{
		return "";
	}
}*/