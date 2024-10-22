using Microsoft.Extensions.Localization;
using OrchardCore.Email.Controllers;
using OrchardCore.Email.Core;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;

namespace OrchardCore.Email;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Tools"], tools => tools
                .Add(S["Email Test"], S["Email Test"].PrefixPosition(), emailTest => emailTest
                    .AddClass("emailtest")
                    .Id("emailtest")
                    .Action(nameof(AdminController.Test), typeof(AdminController).ControllerName(), "OrchardCore.Email")
                    .Permission(Permissions.ManageEmailSettings)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
