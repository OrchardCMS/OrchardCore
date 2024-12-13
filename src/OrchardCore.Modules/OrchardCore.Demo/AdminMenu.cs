using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Demo;

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
            .Add(S["Demo"], "10", demo => demo
                .AddClass("demo").Id("demo")
                .Add(S["This Menu Item 1"], "0", item => item
                    .Add(S["This is Menu Item 1.1"], subItem => subItem
                        .Action("Index", "Admin", "OrchardCore.Demo"))
                    .Add(S["This is Menu Item 1.2"], subItem => subItem
                        .Action("Index", "Admin", "OrchardCore.Demo"))
                    .Add(S["This is Menu Item 1.2"], subItem => subItem
                        .Action("Index", "Admin", "OrchardCore.Demo"))
                )
                .Add(S["This Menu Item 2"], "0", item => item
                    .Add(S["This is Menu Item 2.1"], subItem => subItem
                        .Action("Index", "Admin", "OrchardCore.Demo"))
                    .Add(S["This is Menu Item 2.2"], subItem => subItem
                        .Action("Index", "Admin", "OrchardCore.Demo"))
                    .Add(S["This is Menu Item 3.2"], subItem => subItem
                        .Action("Index", "Admin", "OrchardCore.Demo"))
                )
                .Add(S["This Menu Item 3"], "0", item => item
                    .Add(S["This is Menu Item 3.1"], subItem => subItem
                        .Action("Index", "Admin", "OrchardCore.Demo"))
                    .Add(S["This is Menu Item 3.2"], subItem => subItem
                        .Action("Index", "Admin", "OrchardCore.Demo"))

                )
                .Add(S["Todo (Liquid - Frontend)"], "0", item => item
                    .Action("Index", "Todo", "OrchardCore.Demo")
                )
            );

        return ValueTask.CompletedTask;
    }
}
