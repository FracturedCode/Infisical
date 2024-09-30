using NSwag;
using NSwag.CodeGeneration.OperationNameGenerators;

namespace Net.FracturedCode.Infisical.Pipeline.Generation;

internal class InfisicalOperationNameGenerator : IOperationNameGenerator
{
	public bool SupportsMultipleClients => true;

	// Because for some reason extending and overriding the virtual doesn't work
	private MultipleClientsFromPathSegmentsOperationNameGenerator _innerGenerator = new();

	public string GetClientName(
		OpenApiDocument document,
		string path,
		string httpMethod,
		OpenApiOperation operation
	)
	{
		List<string> components = path
			.Split('/')
			.Where(c => !c.Contains("{") && !string.IsNullOrWhiteSpace(c))
			.ToList();

		components = components.First() switch
		{
			"api" => components.Skip(1).Take(2).ToList(), // ex: ["v1", "secrets"]
			".well-known" => ["well", "known"],
			_ => throw new NotSupportedException("url component not supported")
		};

		return components
			.SelectMany(x => x.Split('-'))
			.Select(x => x.CapitalizeFirstChar())
			.Aggregate((x, y) => $"{x}{y}");
	}

	public string GetOperationName(OpenApiDocument document, string path, string httpMethod, OpenApiOperation operation) =>
		_innerGenerator.GetOperationName(document, path, httpMethod, operation);
}