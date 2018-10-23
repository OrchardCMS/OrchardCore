namespace OrchardCore.Apis.GraphQL
{
    public static class StringExtensions
    {
        public static string ToGraphQLStringFormat(this string value)
        {
            return char.ToLower(value[0]) + value.Substring(1);
        }

        public static string FromGraphQLStringFormat(this string value)
        {
            return char.ToUpper(value[0]) + value.Substring(1);
        }
    }
}