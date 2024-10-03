using System.Diagnostics;
using Humanizer;
using Net.FracturedCode.Infisical.Pipeline.Extensions;
using NSwag;
using NSwag.CodeGeneration.OperationNameGenerators;

namespace Net.FracturedCode.Infisical.Pipeline.Generation;

internal class InfisicalOperationNameGenerator : IOperationNameGenerator
{
	public bool SupportsMultipleClients => true;

	public string GetClientName(
		OpenApiDocument document,
		string path,
		string httpMethod,
		OpenApiOperation operation
	)
	{
		List<string> components = getPathComponents(path);

		components = components.First() switch
		{
			"api" => components.Skip(1).Take(2).ToList(), // ex: ["v1", "secrets"]
			".well-known" => ["well", "known"],
			_ => throw _rootException
		};

		return normalizeAndJoin(components);
	}

	public string GetOperationName(OpenApiDocument document, string path, string httpMethod, OpenApiOperation operation)
	{
		List<string> components = getPathComponents(path);
		components = components.First() switch
		{
			"api" => components.Skip(3).ToList(),
			".well-known" => components.Skip(1).ToList(),
			_ => throw _rootException
		};
		var queryParams = operation.Parameters.Where(x => x.IsRequired && x.Kind == OpenApiParameterKind.Query).ToList();
		bool isQueryBy = queryParams.Count > 0;
		var pathParams = operation.Parameters.Where(x => x.IsRequired && x.Kind == OpenApiParameterKind.Path).ToList();
		var isPathBy = pathParams.Count > 0;
		string operationName = components.Any() switch
		{
			true when isPathBy => normalizeAndJoin(components.Select((c, i) => i == 0 && !c.Contains('{') ? c.Singularize(false) : c).ToList()) + getByClause(queryParams),
			true => normalizeAndJoin(components) + getByClause(queryParams),
			false when !isQueryBy => "",
			false when isQueryBy => $"All{getByClause(queryParams)}",
			_ => throw new UnreachableException()
		};

		return httpMethod + operationName;
	}

	private static string getByClause(List<OpenApiParameter> queryParams)
	{
		if (queryParams.Count < 1)
		{
			return string.Empty;
		}

		return "By" + queryParams.Select(p => p.Name.CapitalizeFirstChar()).Aggregate((x, y) => $"{x}And{y}");
	}

	private static NotSupportedException _rootException => new("root url path component not supported");
	
	private static string normalizeAndJoin(List<string> components)
	{
		components = components.Where(c => !c.Contains('{')).ToList();
		if (!components.Any())
		{
			return string.Empty;
		}
		return components
			.SelectMany(x => x.Split('-'))
			.Select(x => x.CapitalizeFirstChar())
			.Aggregate((x, y) => $"{x}{y}");
	}

	private static List<string> getPathComponents(string path) =>
		path
			.Split('/')
			.Where(c => !string.IsNullOrWhiteSpace(c))
			.ToList();
}