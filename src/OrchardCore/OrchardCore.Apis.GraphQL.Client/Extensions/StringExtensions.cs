using System;

namespace OrchardCore.Apis.GraphQL.Client
{
    internal static class StringExtensions
    {
        public static string ToGraphQLStringFormat(this string value) => Char.ToLower(value[0]) + value[1..];
    }
}
