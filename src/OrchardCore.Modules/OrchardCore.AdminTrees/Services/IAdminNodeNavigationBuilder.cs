using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.AdminTrees.Models;
using OrchardCore.Navigation;

namespace OrchardCore.AdminTrees.Services
{
    public interface IAdminNodeNavigationBuilder
    {
        // This Name will be used to determine if the node passed has to be handled.
        // The builder will handle  only the nodes whose typeName equals this name.
        string Name { get; }

        void BuildNavigation(MenuItem treeNode, NavigationBuilder builder, IEnumerable<IAdminNodeNavigationBuilder> treeNodeBuilders);
        
    }
}
