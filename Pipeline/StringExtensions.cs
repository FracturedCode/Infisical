namespace Net.FracturedCode.Infisical.Pipeline;

// TODO put in library?
internal static class StringExtensions
{
	public static string CapitalizeFirstChar(this string input) =>
		input.Length switch
		{
			< 1 => input,
			1 => input.ToUpper(),
			> 1 => $"{char.ToUpper(input[0])}{input[1..]}",
		};
}