using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services
{
    //this moves to becoming the builder to assemble sitemaps
    public interface ISitemapNodeBuilder
    {
        // This Name will be used to determine if the node passed has to be handled.
        // The builder will handle  only the nodes whose typeName equals this name.
        string Name { get; }

        Task BuildSitemapAsync(SitemapNode treeNode, IEnumerable<ISitemapNodeBuilder> treeNodeBuilders);
        
    }
}
