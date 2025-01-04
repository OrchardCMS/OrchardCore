using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Navigation;

namespace OrchardCore.Tenants;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly ShellSettings _shellSettings;

    internal readonly IStringLocalizer S;

    public AdminMenu(
        ShellSettings shellSettings,
        IStringLocalizer<AdminMenu> stringLocalizer)
    {
        _shellSettings = shellSettings;
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        // Don't add the menu item on non-default tenants.
        if (!_shellSettings.IsDefaultShell())
        {
            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Multi-Tenancy"], "after", tenancy => tenancy
                .AddClass("menu-multitenancy")
                .Id("multitenancy")
                .Add(S["Tenants"], S["Tenants"].PrefixPosition(), tenant => tenant
                    .Action("Index", "Admin", "OrchardCore.Tenants")
                    .Permission(Permissions.ManageTenants)
                    .LocalNav()
                ),
                priority: 1);

        return ValueTask.CompletedTask;
    }
}
