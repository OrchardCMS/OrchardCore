using Microsoft.Extensions.Localization;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Secrets.Controllers;

namespace OrchardCore.Secrets;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (NavigationHelper.UseLegacyFormat())
        {
            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Security"], S["Security"].PrefixPosition(), AddSecrets)
                );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Tools"], tools => tools
                .Add(S["Security"], S["Security"].PrefixPosition(), AddSecrets)
            );

        return ValueTask.CompletedTask;
    }

    // The Secrets items stay together, in this order, among the other security items.
    private void AddSecrets(NavigationBuilder security)
    {
        var position = S["Secrets"].PrefixPosition();

        security
            .Add(S["Secrets"], position + ".1", secrets => secrets
                .AddClass("secrets")
                .Id("secrets")
                .Action(nameof(AdminController.Index), typeof(AdminController).ControllerName(), "OrchardCore.Secrets")
                .Permission(SecretsPermissions.ViewSecrets)
                .LocalNav()
            )
            .Add(S["Migrate Secrets"], position + ".2", migration => migration
                .AddClass("secretsmigration")
                .Id("secretsmigration")
                .Action(nameof(MigrationController.Index), typeof(MigrationController).ControllerName(), "OrchardCore.Secrets")
                .Permission(SecretsPermissions.ManageSecrets)
                .LocalNav()
            )
            .Add(S["Secret Stores"], position + ".3", stores => stores
                .AddClass("secretstores")
                .Id("secretstores")
                .Action(nameof(StoreController.Index), typeof(StoreController).ControllerName(), "OrchardCore.Secrets")
                .Permission(SecretsPermissions.ManageSecrets)
                .LocalNav()
            );
    }
}
