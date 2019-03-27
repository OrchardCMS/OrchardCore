using System;
using GraphQL;
using OrchardCore.ContentManagement.Utilities;

namespace OrchardCore.ContentManagement.GraphQL
{
    public static class StringExtensions
    {
        public static string ToFieldName(this string name)
        {
            return name.TrimEnd("Part").ToCamelCase();
        }
    }
}
