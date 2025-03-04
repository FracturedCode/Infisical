// TODO make a package instead of copypasta
namespace Net.FracturedCode.FracStack.Cloud.AppHost;

public static class PostgresExtensions
{

	private static ReferenceExpression getConnectionUrl(this PostgresDatabaseResource db, bool isInContainer)
	{
		var pg = db.Parent;
		var userNameReference = pg.UserNameParameter is not null
			? ReferenceExpression.Create($"{pg.UserNameParameter}")
			: ReferenceExpression.Create($"postgres");

		// WARNING: this may mess up the manifest for the containers
		var host = isInContainer
			? ReferenceExpression.Create($"{pg.Name}:{pg.PrimaryEndpoint.Property(EndpointProperty.TargetPort)}")
			: ReferenceExpression.Create($"{pg.PrimaryEndpoint.Property(EndpointProperty.Host)}:{pg.PrimaryEndpoint.Property(EndpointProperty.Port)}");

		return ReferenceExpression.Create(
			$"postgresql://{userNameReference}:{pg.PasswordParameter}@{host}/{db.DatabaseName}");
	}
	
	public static IResourceBuilder<T> WithReference<T>(
		this IResourceBuilder<T> builder, IResourceBuilder<PostgresDatabaseResource> db,
		ConnectionStringType connectionStringType = ConnectionStringType.Npgsql,
		string? connectionStringName = null
	) where T : IResourceWithEnvironment
	{
		var connectionStringExpression = connectionStringType switch
		{
			ConnectionStringType.Uri => db.Resource.getConnectionUrl(builder is IResourceBuilder<ContainerResource>),
			ConnectionStringType.Npgsql => db.Resource.ConnectionStringExpression,
			_ => throw new NotSupportedException()
		};
		return builder.WithEnvironment(connectionStringName ?? $"ConnectionStrings__{db.Resource.Name}", connectionStringExpression);
	}
}

public enum ConnectionStringType
{
	Uri,
	Npgsql
}