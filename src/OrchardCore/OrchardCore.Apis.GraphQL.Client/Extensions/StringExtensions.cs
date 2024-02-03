namespace OrchardCore.Apis.GraphQL.Client;

internal static class StringExtensions
{
    public static string ToGraphQLStringFormat(this string value) => char.ToLower(value[0]) + value[1..];
}
