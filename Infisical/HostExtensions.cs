using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Net.FracturedCode.Infisical;

public static partial class HostExtensions
{
	public static IHostApplicationBuilder AddInfisicalClients(this IHostApplicationBuilder builder)
	{
		builder.Services.AddOptions<InfisicalSettings>()
			.BindConfiguration("Infisical")
			.ValidateDataAnnotations()
			.ValidateOnStart();
		return builder.addAllHttpClients();
	}

	private static partial IHostApplicationBuilder addAllHttpClients(this IHostApplicationBuilder builder);
}