using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Tenants.Recipes;

/// <summary>
/// Filters out recipes that are only available for the Default tenant.
/// Recipes with specific tags (e.g., "saas", "multi-tenant") are only shown on the Default tenant.
/// </summary>
public sealed class DefaultTenantRecipeFilter : IRecipeFilter
{
    /// <summary>
    /// Tags that indicate a recipe should only be available on the Default tenant.
    /// </summary>
    private static readonly HashSet<string> _defaultTenantOnlyTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "saas",
        "multi-tenant",
        "default",
        "default-tenant-only",
    };

    private readonly ShellSettings _shellSettings;

    public DefaultTenantRecipeFilter(ShellSettings shellSettings)
    {
        _shellSettings = shellSettings;
    }

    /// <inheritdoc />
    public int Order => 100;

    /// <inheritdoc />
    public ValueTask<bool> ShouldIncludeAsync(RecipeDescriptor recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        // If we're on the Default tenant, include all recipes
        if (_shellSettings.IsDefaultShell())
        {
            return ValueTask.FromResult(true);
        }

        // Check if the recipe has any tags that restrict it to the Default tenant
        if (recipe.Tags is not null)
        {
            foreach (var tag in recipe.Tags)
            {
                if (_defaultTenantOnlyTags.Contains(tag))
                {
                    return ValueTask.FromResult(false);
                }
            }
        }

        // Also check the recipe name for known Default-tenant-only recipes
        if (string.Equals(recipe.Name, "SaaS", StringComparison.OrdinalIgnoreCase))
        {
            return ValueTask.FromResult(false);
        }

        return ValueTask.FromResult(true);
    }
}
