using System;
using System.Collections.Generic;
using System.Text;
using GraphQL.Builders;
using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL
{
    public static class FieldNameExtensions
    {
        private const string _originalNameMetaDataKey = "OriginalFieldName";

        public static void WithOriginalNameMetaData(this IProvideMetadata type, string originalName)
        {
            type.Metadata[_originalNameMetaDataKey] = originalName;
        }
    }
}
