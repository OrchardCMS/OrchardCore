using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.Apis.GraphQL
{
    internal static class StringExtensions
    {
        public static string ToGraphQLStringFormat(this string value) {
            return char.ToLower(value[0]) + value.Substring(1);
        }
    }
}
