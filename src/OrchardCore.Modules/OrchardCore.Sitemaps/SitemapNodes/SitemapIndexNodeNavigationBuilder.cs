using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Sitemaps.Services;
using OrchardCore.Navigation;

namespace OrchardCore.Sitemaps.SitemapNodes
{
    //this class is unecesary, this is the bit that builds the custom admin menu
    //it needs to become a sitemap provider builder
    //public class SitemapIndexNodeNavigationBuilder : ISitemapNodeNavigationBuilder
    //{
    //    private readonly ILogger<SitemapIndexNodeNavigationBuilder> _logger;

    //    public SitemapIndexNodeNavigationBuilder(ILogger<SitemapIndexNodeNavigationBuilder> logger)
    //    {
    //        _logger = logger;
    //    }

    //    public string Name => typeof(SitemapIndexNode).Name;


    //    public Task BuildNavigationAsync(MenuItem menuItem, NavigationBuilder builder, IEnumerable<ISitemapNodeNavigationBuilder> treeNodeBuilders)
    //    {
    //        var node = menuItem as SitemapIndexNode;

    //        if (node == null || string.IsNullOrEmpty(node.LinkText) || !node.Enabled)
    //        {
    //            return Task.CompletedTask;
    //        }

    //        return builder.AddAsync(new LocalizedString(node.LinkText, node.LinkText), async itemBuilder =>
    //        {

    //            itemBuilder.Priority(node.Priority);
    //            itemBuilder.Position(node.Position);

    //            // Add adminNode's IconClass property values to menuItem.Classes. 
    //            // Add them with a prefix so that later the shape template can extract them to use them on a <i> tag.              
    //            node.IconClass?.Split(' ').ToList().ForEach(c => itemBuilder.AddClass("icon-class-" + c));


    //            // Let children build themselves inside this MenuItem
    //            // todo: this logic can be shared by all TreeNodeNavigationBuilders
    //            foreach (var childTreeNode in menuItem.Items)
    //            {
    //                try
    //                {
    //                    var treeBuilder = treeNodeBuilders.Where(x => x.Name == childTreeNode.GetType().Name).FirstOrDefault();
    //                    await treeBuilder.BuildNavigationAsync(childTreeNode, itemBuilder, treeNodeBuilders);
    //                }
    //                catch (Exception e)
    //                {
    //                    _logger.LogError(e, "An exception occurred while building the '{MenuItem}' child Menu Item.", childTreeNode.GetType().Name);
    //                }
    //            }
    //        });
    //    }
    //}
}
