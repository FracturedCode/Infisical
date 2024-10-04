using Microsoft.Extensions.DependencyInjection;

namespace Net.FracturedCode.Infisical;

/// <summary>
/// DI extensions for help registering the Infisical Clients
/// </summary>
public static partial class ServiceCollectionExtensions
{
	public static IServiceCollection AddInfisicalClients(this IServiceCollection services, Action<IHttpClientBuilder>? customHttpClientBuilder = null)
	{
		services.AddOptions<InfisicalSettings>()
			.BindConfiguration("Infisical")
			.ValidateDataAnnotations()
			.ValidateOnStart();
		return services.addAllHttpClients(customHttpClientBuilder);
	}
	
	// TODO make an extension to register for only common secret queries

	internal static partial IServiceCollection addAllHttpClients(this IServiceCollection services, Action<IHttpClientBuilder>? customHttpClientBuilder);
}