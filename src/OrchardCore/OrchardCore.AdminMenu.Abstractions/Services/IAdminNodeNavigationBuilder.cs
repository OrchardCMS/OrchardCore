using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Navigation;

namespace OrchardCore.AdminMenu.Services
{
    public interface IAdminNodeNavigationBuilder
    {
        // This Name will be used to determine if the node passed has to be handled.
        // The builder will handle only the nodes whose type name equals this name.
        string Name { get; }

        Task BuildNavigationAsync(MenuItem treeNode, NavigationBuilder builder, IEnumerable<IAdminNodeNavigationBuilder> treeNodeBuilders);
    }
}
