using System.CodeDom.Compiler;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Net.FracturedCode.Infisical.SourceGenerators;

[Generator]
public class HttpClientHostExtensionsGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var clientTypes = context.SyntaxProvider
			.CreateSyntaxProvider(isClientType, getClientClass)
			.Collect();
		
		context.RegisterSourceOutput(clientTypes, (productionContext, clientTypeList) =>
		{
			if (!clientTypeList.Any())
			{
				return;
			}
			productionContext.AddSource("HttpClientHostExtensions.g.cs", SourceText.From($$"""
				using Microsoft.Extensions.DependencyInjection;
				using Microsoft.Extensions.Hosting;
				using Net.FracturedCode.Infisical.Clients;
				
				namespace Net.FracturedCode.Infisical;
				
				public static partial class HostExtensions
				{
					[global::System.CodeDom.Compiler.GeneratedCodeAttribute("{{typeof(HttpClientHostExtensionsGenerator).FullName}}", "1.0.0")]
					private static partial IHostApplicationBuilder addAllHttpClients(this IHostApplicationBuilder builder)
					{
						builder{{clientTypeList
							.Select(c => $".Services.AddHttpClient<I{c.ClassName}, {c.ClassName}>()")
							.Aggregate((x, y) => $"{x}\n			{y}")}};
						return builder;
					}
				}
				""", Encoding.UTF8));
		});
	}

	private record Client
	{
		public Client(string className)
		{
			ClassName = className;
		}

		public string ClassName { get; }
	}

	private static Client getClientClass(GeneratorSyntaxContext context, CancellationToken _)
	{
		var cds = (ClassDeclarationSyntax)context.Node;
		return new Client(cds.Identifier.ValueText);
	}

	private static bool isClientType(SyntaxNode node, CancellationToken _)
	{
		return node is ClassDeclarationSyntax classDecl &&
			(classDecl.Parent as NamespaceDeclarationSyntax)?.Name.ToString().EndsWith("Clients") == true;
	}
}