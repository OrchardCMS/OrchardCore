using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Navigation;

namespace OrchardCore.Sitemaps.Services
{
    public interface ISitemapNodeNavigationBuilder
    {
        // This Name will be used to determine if the node passed has to be handled.
        // The builder will handle  only the nodes whose typeName equals this name.
        string Name { get; }

        Task BuildNavigationAsync(MenuItem treeNode, NavigationBuilder builder, IEnumerable<ISitemapNodeNavigationBuilder> treeNodeBuilders);
        
    }
}
