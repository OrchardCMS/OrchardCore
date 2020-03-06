using GraphQL.Types;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    public interface ISiteGraphBuilder
    {
        void Build(SiteGraphType siteType);
    }
}
