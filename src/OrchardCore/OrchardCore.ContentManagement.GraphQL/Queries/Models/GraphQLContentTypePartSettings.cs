using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Models
{
    public class GraphQLContentTypePartSettings
    {
        public bool CollapseFieldsToParent { get; set; }
    }

    public class GraphQLContentOptions
    {
        public IEnumerable<GraphQLContentTypeOption> ContentTypeOptions { get; set; }
            = Enumerable.Empty<GraphQLContentTypeOption>();
    }

    public class GraphQLContentTypeOption
    {
        public string ContentType { get; set; }

        public bool Collapse { get; set; }

        public IEnumerable<GraphQLContentPartOption> PartOptions { get; set; }
            = Enumerable.Empty<GraphQLContentPartOption>();
    }

    public class GraphQLContentPartOption
    {
        public Type Type { get; set; }

        public bool Collapse { get; set; }
    }
}