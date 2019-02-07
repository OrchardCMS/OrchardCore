using System.Collections.Generic;
using System.Linq;
using OrchardCore.ContentManagement.GraphQL.Settings;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Options
{
    public class GraphQLContentOptions
    {
        public IEnumerable<GraphQLContentTypeOption> ContentTypeOptions { get; set; }
            = Enumerable.Empty<GraphQLContentTypeOption>();

        public IEnumerable<GraphQLContentPartOption> PartOptions { get; set; }
            = Enumerable.Empty<GraphQLContentPartOption>();

        /// <summary>
        /// Collapsing works at a heirachy
        ///
        /// If the Content Type is marked at collapsed, then all parts are collapsed.
        /// If the Content Type is not marked collapsed, then it falls down to the content type under it.
        /// If the Content Part at a top level is marked collapsed, then it will trump above.
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        public bool ShouldCollapse(ContentTypePartDefinition definition)
        {
            if (IsCollapsedByDefault(definition))
            {
                return true;
            }

            var settings = definition.GetSettings<GraphQLContentTypePartSettings>();

            if (settings.Collapse)
            {
                return true;
            }

            return false;
        }

        public bool IsCollapsedByDefault(ContentTypePartDefinition definition)
        {
            var contentType = definition.ContentTypeDefinition.Name;
            var partName = definition.PartDefinition.Name;

            if (contentType == partName)
            {
                return true;
            }

            var contentTypeOption = ContentTypeOptions.FirstOrDefault(ctp => ctp.ContentType == contentType);

            if (contentTypeOption != null)
            {
                if (contentTypeOption.Collapse)
                {
                    return true;
                }

                var contentTypePartOption = contentTypeOption.PartOptions.FirstOrDefault(p => p.Name == partName);

                if (contentTypePartOption != null)
                {
                    if (contentTypePartOption.Collapse)
                    {
                        return true;
                    }
                }
            }

            var contentPartOption = PartOptions.FirstOrDefault(p => p.Name == partName);

            if (contentPartOption != null)
            {
                if (contentPartOption.Collapse)
                {
                    return true;
                }
            }

            return false;
        }

        public bool ShouldIgnore(ContentTypePartDefinition definition)
        {
            if (IsIgnoredByDefault(definition))
            {
                return true;
            }

            var settings = definition.GetSettings<GraphQLContentTypePartSettings>();

            if (settings.Ignore)
            {
                return true;
            }

            return false;
        }

        public bool IsIgnoredByDefault(ContentTypePartDefinition definition)
        {
            var contentType = definition.ContentTypeDefinition.Name;
            var partName = definition.PartDefinition.Name;

            if (contentType == partName)
            {
                return true;
            }

            var contentTypeOption = ContentTypeOptions.FirstOrDefault(ctp => ctp.ContentType == contentType);

            if (contentTypeOption != null)
            {
                if (contentTypeOption.Ignore)
                {
                    return true;
                }

                var contentTypePartOption = contentTypeOption.PartOptions.FirstOrDefault(p => p.Name == partName);

                if (contentTypePartOption != null)
                {
                    if (contentTypePartOption.Ignore)
                    {
                        return true;
                    }
                }
            }

            var contentPartOption = PartOptions.FirstOrDefault(p => p.Name == partName);

            if (contentPartOption != null)
            {
                if (contentPartOption.Ignore)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class GraphQLContentTypeOption
    {
        public string ContentType { get; set; }

        public bool Collapse { get; set; }

        public bool Ignore { get; set; }

        public IEnumerable<GraphQLContentPartOption> PartOptions { get; set; }
            = Enumerable.Empty<GraphQLContentPartOption>();
    }

    public class GraphQLContentPartOption
    {
        public string Name { get; set; }

        public bool Collapse { get; set; }

        public bool Ignore { get; set; }
    }
}