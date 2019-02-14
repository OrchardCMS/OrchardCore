using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.ContentManagement.GraphQL.Options
{
    public class GraphQLContentTypeOption
    {
        public string ContentType { get; set; }

        public bool Collapse { get; set; }

        public bool Ignore { get; set; }

        public IEnumerable<GraphQLContentPartOption> PartOptions { get; set; }
            = Enumerable.Empty<GraphQLContentPartOption>();

        public GraphQLContentTypeOption ConfigurePart<TContentPart>(Action<GraphQLContentPartOption> action)
            where TContentPart : ContentPart
        {
            var option = new GraphQLContentPartOption<TContentPart>();

            action(option);

            PartOptions = PartOptions.Union(new[] { option });

            return this;
        }

        public GraphQLContentTypeOption ConfigurePart(string partName, Action<GraphQLContentPartOption> action)
        {
            var option = new GraphQLContentPartOption { Name = partName };

            action(option);

            PartOptions = PartOptions.Union(new[] { option });

            return this;
        }
    }
}