using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.AdminTrees.Models;
using OrchardCore.AdminTrees.Services;
using OrchardCore.AdminTrees.AdminNodes;
using OrchardCore.Navigation;
using System.Threading.Tasks;

namespace OrchardCore.AdminTrees.AdminNodes
{
    public class LinkAdminNodeNavigationBuilder : IAdminNodeNavigationBuilder
    {
        private readonly ILogger<LinkAdminNodeNavigationBuilder> _logger;

        public LinkAdminNodeNavigationBuilder(ILogger<LinkAdminNodeNavigationBuilder> logger)
        {
            _logger = logger;
        }

        public string Name => typeof(LinkAdminNode).Name;


        public Task BuildNavigationAsync(MenuItem menuItem, NavigationBuilder builder, IEnumerable<IAdminNodeNavigationBuilder> treeNodeBuilders)
        {
            var ltn = menuItem as LinkAdminNode;

            if ((ltn == null) ||( !ltn.Enabled))
            {
                return Task.CompletedTask;
            }

            builder.Add(new LocalizedString(ltn.LinkText, ltn.LinkText), async itemBuilder => {

                // Add the actual link
                itemBuilder.Url(ltn.LinkUrl);
                ltn.CustomClasses.ToList().ForEach( x => itemBuilder.AddClass(x));

                // Add the other ITreeNodeNavigationBuilder build themselves as children of this link

                // Let children build themselves inside this MenuItem
                // todo: this logic can be shared by all TreeNodeNavigationBuilders
                foreach (var childTreeNode in menuItem.Items)
                {
                    try
                    {
                        var treeBuilder = treeNodeBuilders.Where(x => x.Name == childTreeNode.GetType().Name).FirstOrDefault();
                        await treeBuilder.BuildNavigationAsync(childTreeNode, itemBuilder, treeNodeBuilders);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "An exception occurred while building the '{MenuItem}' child Menu Item.", childTreeNode.GetType().Name);
                    }
                }
            });

            return Task.CompletedTask;
        }
    }
}
