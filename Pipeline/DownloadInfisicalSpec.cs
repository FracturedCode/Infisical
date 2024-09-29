using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Exceptions;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Modules;
using Polly;
using Polly.Retry;

namespace Net.FracturedCode.Infisical.Pipeline;

// TODO may be better to use aspire testing framework
// https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/testing?pivots=xunit
public class DownloadInfisicalSpec : Module<string>
{
	protected override async Task<string?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
	{
		var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		int port = await findFreeTcpPort();

		DotNetRunOptions runOptions = new()
		{
			Project = context.Git().RootDirectory.GetFiles(f => f.Name == "AppHost.csproj").Single(),
			Arguments = ["--port", port.ToString()],
			ThrowOnNonZeroExitCode = false
		};

		var appHostHandle = context.DotNet().Run(runOptions, cts.Token);
		
		string specContent = await new ResiliencePipelineBuilder<string>()
			.AddTimeout(TimeSpan.FromSeconds(60))
			.AddRetry(new RetryStrategyOptions<string>
			{
				Delay = TimeSpan.FromMilliseconds(250),
				ShouldHandle = new PredicateBuilder<string>()
					// https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient.sendasync
					// Only handle network issues and request timeouts
					.Handle<HttpRequestException>()
					.Handle<TaskCanceledException>()
			})
			.Build()
			.ExecuteAsync(
				async token =>
				{
					if (appHostHandle.IsCompleted && !token.IsCancellationRequested)
					{
						context.Logger.LogCritical("AppHost exited early. AppHost CommandResult: {commandResult}",
							JsonSerializer.Serialize(appHostHandle.Result));
						throw new Exception("AppHost exited early.");
					}

					return await context.Http.HttpClient.GetStringAsync($"http://localhost:{port}/api/docs/json", token);
				},
				cts.Token
			);

		await cts.CancelAsync();
		context.Logger.LogInformation("Downloaded Infisical JSON spec. Length: {length}", specContent.Length);
		try
		{
			await appHostHandle;
		}
		catch (CommandException e) when (e.InnerException is OperationCanceledException) { }

		return specContent;
	}

	private static async Task<int> findFreeTcpPort()
	{
		return await Task.Run(() =>
		{
			TcpListener listener = new(IPAddress.Loopback, 0);
			listener.Start();
			int port = ((IPEndPoint)listener.LocalEndpoint).Port;
			listener.Stop();
			return port;
		});
	}
}