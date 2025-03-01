using System.Security.Cryptography;
using Aspire.Hosting;
using Net.FracturedCode.FracStack.Cloud.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

const string infisicalDbName = "infisicaldb";

var pgPw = builder.AddPersistentSecret("postgresql-password",
	() => new Guid(RandomNumberGenerator.GetBytes(16)).ToString());
var infisicalDb = builder.AddPostgres("postgres", null, pgPw)
	.WithEnvironment("POSTGRES_DB", infisicalDbName)
	.AddDatabase(infisicalDbName);

var redis = builder.AddRedis("redis-infisical");

IResourceBuilder<ContainerResource> addInfisicalContainer(string name) =>
	builder.AddContainer(name, "infisical/infisical", "v0.112.0-postgres") // Change the version in Infisical.csproj as well
		.WithReference(infisicalDb, ConnectionStringType.Uri, "DB_CONNECTION_URI");

var encryptionKey = builder.AddPersistentSecret("infisical-encryptionKey", () => RandomNumberGenerator.GetHexString(32));
var authSecret = builder.AddPersistentSecret("infisical-authSecret", () => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
addInfisicalContainer("infisical")
	.WithEnvironment("REDIS_URL", () => $"{redis.Resource.Name}:{redis.Resource.PrimaryEndpoint.TargetPort}")
	.WithEnvironment("ENCRYPTION_KEY", encryptionKey)
	.WithEnvironment("AUTH_SECRET", authSecret)
	.WithEnvironment("PORT", "80")
	.WithHttpEndpoint(targetPort: 80, port: int.Parse(args.LastOrDefault() ?? "5005"), name: "infisical");

await builder.Build().RunAsync();