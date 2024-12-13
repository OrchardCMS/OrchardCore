using Microsoft.Extensions.Localization;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Navigation;

namespace OrchardCore.AdminMenu;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly AdminMenuNavigationProvidersCoordinator _adminMenuNavigationProviderCoordinator;

    internal readonly IStringLocalizer S;

    public AdminMenu(
        AdminMenuNavigationProvidersCoordinator adminMenuNavigationProviderCoordinator,
        IStringLocalizer<AdminMenu> stringLocalizer)
    {
        _adminMenuNavigationProviderCoordinator = adminMenuNavigationProviderCoordinator;
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        // Configuration and settings menus for the AdminMenu module.
        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Admin Menus"], S["Admin Menus"].PrefixPosition(), adminMenu => adminMenu
                    .Permission(Permissions.ManageAdminMenu)
                    .Action("List", "Menu", "OrchardCore.AdminMenu")
                    .LocalNav()
                )
            );

        // This is the entry point for the adminMenu: dynamically generated custom admin menus.
        return _adminMenuNavigationProviderCoordinator.BuildNavigationAsync(NavigationConstants.AdminMenuId, builder);
    }
}
