using System.ComponentModel.DataAnnotations;

namespace Net.FracturedCode.Infisical;

public class InfisicalSettings
{
	[Required]
	[Url]
	public required string Url { get; init; }
	[Required]
	public required string ClientId { get; init; }
	[Required]
	public required string ClientSecret { get; init; }
}