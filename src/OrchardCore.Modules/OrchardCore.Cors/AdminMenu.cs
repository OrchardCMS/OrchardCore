using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Cors;

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
                .Add(S["CORS"], S["CORS"].PrefixPosition(), entry => entry
                    .AddClass("cors")
                    .Id("cors")
                    .Action("Index", "Admin", "OrchardCore.Cors")
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
