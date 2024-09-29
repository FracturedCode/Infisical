using CliWrap;

// TODO make a package instead of copypasta
namespace Net.FracturedCode.FracStack.Cloud.AppHost;

internal static class ParameterExtensions
{
	/// <summary>
	/// Adds a secret Parameter to the builder.
	/// Sources a default value and saves it with dotnet user-secrets if it does not already exist.
	/// </summary>
	/// <param name="builder"></param>
	/// <param name="name"></param>
	/// <param name="sourceDefaultValueFn">The function invoked to source the secret if it does not already exist.</param>
	/// <remarks>
	/// The strange pattern with param is bc AddParameter does validation.
	/// We want validation before we produce side effects (writing user secrets).
	/// However, if AddParameter is called at the top of the function, it would fail if the secret wasn't present in Configuration
	/// </remarks>
	/// <returns></returns>
	public static IResourceBuilder<ParameterResource> AddPersistentSecret(
		this IDistributedApplicationBuilder builder,
		string name,
		Func<string> sourceDefaultValueFn)
	{
		IResourceBuilder<ParameterResource>? param = null;
		string configKey = $"Parameters:{name}";
		string? configValue = builder.Configuration[configKey];
		if (configValue is null)
		{
			configValue = sourceDefaultValueFn();
			builder.Configuration[configKey] = configValue;
			param = builder.AddParameter(name, true);
			var cmd = Cli.Wrap("dotnet").WithArguments(["user-secrets", "set", configKey, configValue]);
			Task.Run(() => cmd.ExecuteAsync()).GetAwaiter().GetResult().GetAwaiter().GetResult();
		}

		return param ?? builder.AddParameter(name, true);
	}
}