using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Logging;
using ModularPipelines.Context;
using ModularPipelines.Docker.Extensions;
using ModularPipelines.Docker.Options;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Exceptions;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Modules;
using Polly;
using Polly.Retry;
using YamlDotNet.Core;

namespace Net.FracturedCode.Infisical.Pipeline.Modules;

public class DownloadInfisicalSpec : Module<string>
{
	protected override async Task<string?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
	{
		/*
		 * Verified:
		 * The initial ssh connection is successful
		 * The aspire dashboard is up
		 * normal containers can communicate with the host
		 */
		int port = 5005;
		var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

		DotNetRunOptions runOptions = new()
		{
			Project = context.Git().RootDirectory.GetFiles(f => f.Name == "AppHost.csproj").Single(),
			Arguments = ["--port", port.ToString()],
			ThrowOnNonZeroExitCode = false,
			LaunchProfile = "http",
			EnvironmentVariables = new Dictionary<string, string?>
			{
				{ "ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true" },
				{ "Dashboard__Frontend__AuthMode", "Unsecured" },
				{ "DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS", "true" }
			}
		};
		
		var appHostHandle = context.DotNet().Run(runOptions, cts.Token);

		string key = """
		-----BEGIN OPENSSH PRIVATE KEY-----
		b3BlbnNzaC1rZXktdjEAAAAABG5vbmUAAAAEbm9uZQAAAAAAAAABAAAAMwAAAAtzc2gtZW
		QyNTUxOQAAACB6JeTfwZsRe/Ehz2FyzxxeCKjAwlLkrXvCXUk0sh7oywAAAJD0ckbT9HJG
		0wAAAAtzc2gtZWQyNTUxOQAAACB6JeTfwZsRe/Ehz2FyzxxeCKjAwlLkrXvCXUk0sh7oyw
		AAAEAqHGMM9uAlGO5pRRBZ5DNUBALUT+rQfZBqAL9VJKjNc3ol5N/BmxF78SHPYXLPHF4I
		qMDCUuSte8JdSTSyHujLAAAACGdoQHZlbm9tAQIDBAU=
		-----END OPENSSH PRIVATE KEY-----
		
		""";

		var sshKeyFile = context.FileSystem.GetFolder("/home/runner").CreateFolder(".ssh").CreateFile("venom");
		await Cli.Wrap("chmod").WithArguments(["0600", sshKeyFile]).ExecuteAsync(cancellationToken);
		await sshKeyFile.WriteAsync(key ?? throw new Exception("Missing secret"), cancellationToken);

		var sshTunnel = Cli.Wrap("ssh").WithArguments([
			"-R", "15252:localhost:15252",
			"-N", "-f",
			"gh@venom.fracturedcode.net",
			"-p", "69",
			"-i", sshKeyFile,
			"-o", "StrictHostKeyChecking=accept-new"
		])
		.ExecuteAsync(cts.Token);

		string specContent = await new ResiliencePipelineBuilder<string>()
			.AddTimeout(TimeSpan.FromSeconds(1000))
			.AddRetry(new RetryStrategyOptions<string>
			{
				Delay = TimeSpan.FromMilliseconds(250),
				MaxRetryAttempts = int.MaxValue,
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

	private static async Task<int> findFreeTcpPort(CancellationToken ct)
	{
		return await Task.Run(() =>
		{
			TcpListener listener = new(IPAddress.Loopback, 0);
			listener.Start();
			int port = ((IPEndPoint)listener.LocalEndpoint).Port;
			listener.Stop();
			return port;
		}, ct);
	}
}