using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Recipes;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizer S;

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
                    .Add(S["Recipes"], S["Recipes"].PrefixPosition(), recipes => recipes
                        .AddClass("recipes")
                        .Id("recipes")
                        .Permission(RecipePermissions.ManageRecipes)
                        .Action("Index", "Admin", "OrchardCore.Recipes")
                        .LocalNav()
                    )
                );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Tools"], tools => tools
                .Add(S["Recipes"], S["Recipes"].PrefixPosition(), recipes => recipes
                    .AddClass("recipes")
                    .Id("recipes")
                    .Permission(RecipePermissions.ManageRecipes)
                    .Action("Index", "Admin", "OrchardCore.Recipes")
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
