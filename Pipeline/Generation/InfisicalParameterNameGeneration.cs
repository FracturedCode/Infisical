using NSwag;
using NSwag.CodeGeneration;

namespace Net.FracturedCode.Infisical.Pipeline.Generation;

internal class InfisicalParameterNameGeneration : DefaultParameterNameGenerator
{
	public new string Generate(OpenApiParameter parameter, IEnumerable<OpenApiParameter> allParameters)
	{
		return base.Generate(parameter, allParameters).Replace("@$glob", "glob");
	}
}