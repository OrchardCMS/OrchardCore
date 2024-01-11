using System;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    public class IndexAlias
    {
        public string Alias { get; set; }

        public string Index { get; set; }

        public Type IndexType { get; set; }
    }
}

