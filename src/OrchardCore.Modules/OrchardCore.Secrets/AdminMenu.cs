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
                    .Add(S["Security"], S["Security"].PrefixPosition(), security => security
                        .Add(S["Secrets"], S["Secrets"].PrefixPosition(), secrets => secrets
                            .AddClass("secrets")
                            .Id("secrets")
                            .Action(nameof(AdminController.Index), typeof(AdminController).ControllerName(), "OrchardCore.Secrets")
                            .Permission(SecretsPermissions.ViewSecrets)
                            .LocalNav()
                        )
                        .Add(S["Secret Stores"], S["Secret Stores"].PrefixPosition(), stores => stores
                            .AddClass("secretstores")
                            .Id("secretstores")
                            .Action(nameof(StoreController.Index), typeof(StoreController).ControllerName(), "OrchardCore.Secrets")
                            .Permission(SecretsPermissions.ManageSecrets)
                            .LocalNav()
                        )
                    )
                );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Tools"], tools => tools
                .Add(S["Security"], S["Security"].PrefixPosition(), security => security
                    .Add(S["Secrets"], S["Secrets"].PrefixPosition(), secrets => secrets
                        .AddClass("secrets")
                        .Id("secrets")
                        .Action(nameof(AdminController.Index), typeof(AdminController).ControllerName(), "OrchardCore.Secrets")
                        .Permission(SecretsPermissions.ViewSecrets)
                        .LocalNav()
                    )
                    .Add(S["Secret Stores"], S["Secret Stores"].PrefixPosition(), stores => stores
                        .AddClass("secretstores")
                        .Id("secretstores")
                        .Action(nameof(StoreController.Index), typeof(StoreController).ControllerName(), "OrchardCore.Secrets")
                        .Permission(SecretsPermissions.ManageSecrets)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
