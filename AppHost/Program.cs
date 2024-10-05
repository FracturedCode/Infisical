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
	builder.AddContainer(name, "infisical/infisical", "v0.83.0-postgres")
		.WithContainerRuntimeArgs("--net=host")
		.WithReference(infisicalDb, ConnectionStringType.Uri, "DB_CONNECTION_URI");

var infisicalMigrations = addInfisicalContainer("infisical-migrations")
	.WaitFor(infisicalDb)
	.WithArgs("npm run migration:latest".Split(' '));
var encryptionKey = builder.AddPersistentSecret("infisical-encryptionKey", () => RandomNumberGenerator.GetHexString(32));
var authSecret = builder.AddPersistentSecret("infisical-authSecret", () => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
addInfisicalContainer("infisical")
	.WaitForCompletion(infisicalMigrations)
	.WaitFor(redis)
	.WithEnvironment("REDIS_URL", () => $"{redis.Resource.PrimaryEndpoint.Host}:{redis.Resource.PrimaryEndpoint.Port}")
	.WithEnvironment("ENCRYPTION_KEY", encryptionKey)
	.WithEnvironment("AUTH_SECRET", authSecret)
	.WithEnvironment("PORT", "80")
	.WithHttpEndpoint(targetPort: 80, port: int.Parse(args.LastOrDefault() ?? "5005"), name: "infisical");

await builder.Build().RunAsync();