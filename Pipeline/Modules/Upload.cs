using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Modules;

namespace Net.FracturedCode.Infisical.Pipeline.Modules;

[DependsOn<Pack>]
[NugetIsConfiguredMandatoryCondition]
public class Upload(IOptions<NugetOptions> nugetOptions) : Module
{
	protected override async Task<IDictionary<string, object>?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
	{
		await context.DotNet().Nuget.Push(new DotNetNugetPushOptions
		{
			ApiKey = nugetOptions.Value.ApiKey,
			Path = (await GetModule<Pack>()).Value ?? throw new ArgumentNullException(),
			Source = $"https://api{(nugetOptions.Value.PushToProd ? ".nuget" : "int.nugettest")}.org/v3/index.json"
		}, cancellationToken);
		return await NothingAsync();
	}
}

public record NugetOptions
{
	public string? ApiKey { get; init; }
	public bool PushToProd { get; init; }
}

public class NugetIsConfiguredMandatoryCondition : MandatoryRunConditionAttribute
{
	public override Task<bool> Condition(IPipelineHookContext pipelineContext) =>
		Task.FromResult(pipelineContext.Get<IOptions<NugetOptions>>()?.Value.ApiKey is not null);
}