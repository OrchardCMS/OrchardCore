using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes;

/// <summary>
/// Describes the breadcrumb trail of the recipes screen.
/// </summary>
public sealed class RecipesBreadcrumbProvider : NamedBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_listRouteValues = new()
    {
        { "area", "OrchardCore.Recipes" },
    };

    internal readonly IStringLocalizer S;

    public RecipesBreadcrumbProvider(IStringLocalizer<RecipesBreadcrumbProvider> stringLocalizer)
        : base(RecipesConstants.List)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(BreadcrumbBuilder builder)
    {
        builder.Add(S["Recipes"], item => item
            .Id("Recipes")
            .Action("Index", "Admin", s_listRouteValues)
            .Permission(RecipePermissions.ManageRecipes));

        return ValueTask.CompletedTask;
    }
}
