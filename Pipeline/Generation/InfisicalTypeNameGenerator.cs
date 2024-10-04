using NJsonSchema;

namespace Net.FracturedCode.Infisical.Pipeline.Generation;

internal class InfisicalTypeNameGenerator : ITypeNameGenerator
{
	private readonly DefaultTypeNameGenerator _defaultTypeNameGenerator = new();
	
	public string Generate(JsonSchema schema, string? typeNameHint, IEnumerable<string> reservedTypeNames)
	{
		return _defaultTypeNameGenerator.Generate(schema, typeNameHint, [..reservedTypeNames, "Type"]);
	}
}