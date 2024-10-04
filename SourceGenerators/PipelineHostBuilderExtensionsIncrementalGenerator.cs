using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Net.FracturedCode.Infisical.SourceGenerators;

[Generator]
public class PipelineHostBuilderExtensionsIncrementalGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var moduleTypes = context.SyntaxProvider
			.CreateSyntaxProvider(isModuleClass, getModuleClass)
			.Collect();
		
		
		context.RegisterSourceOutput(moduleTypes, (c, moduleTypeList) =>
		{
			string code = $$"""
				using System;
				using ModularPipelines.Host;
				using Net.FracturedCode.Infisical.Pipeline.Modules;
				
				namespace Net.FracturedCode.Infisical.Pipeline;
				
				public static partial class PipelineHostBuilderExtensions
				{
					[global::System.CodeDom.Compiler.GeneratedCodeAttribute("{{typeof(PipelineHostBuilderExtensionsIncrementalGenerator).FullName}}", "1.0.0")]
					public static partial PipelineHostBuilder AddAllModules(this PipelineHostBuilder builder)
					{
						{{moduleTypeList
							.Select(m => $"builder.AddModule<{m.ClassName}>();")
							.Aggregate((x, y) => $"{x}\n		{y}")}}
						return builder;
					}
				}
				""";

			c.AddSource("ModuleLoader.g.cs", SourceText.From(code, Encoding.UTF8));
		});
	}

	private record Module
	{
		public Module(string className)
		{
			ClassName = className;
		}
		public string ClassName { get; }
	}

	private static bool isModuleClass(SyntaxNode node, CancellationToken _)
	{
		return node is ClassDeclarationSyntax classDecl &&
			classDecl.BaseList?.Types.Any(t => t.ToString().Contains("Module")) == true;
	}

	private static Module getModuleClass(GeneratorSyntaxContext context, CancellationToken _)
	{
		var cds = (ClassDeclarationSyntax)context.Node;
		return new Module(cds.Identifier.ValueText);
	}
}