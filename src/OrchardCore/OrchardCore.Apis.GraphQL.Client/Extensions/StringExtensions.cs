namespace OrchardCore.Apis.GraphQL.Client
{
    internal static class StringExtensions
    {
        public static string ToGraphQLStringFormat(this string value)
        {
            return char.ToLower(value[0]) + value.Substring(1);
        }
    }
}
