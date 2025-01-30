using OrchardCore.Security.Permissions;

namespace OrchardCore.Recipes;

public sealed class RecipesPermissionProvider : IPermissionProvider
{
    private readonly IEnumerable<Permission> _allPermissions =
    [
        RecipePermissions.ManageRecipes,
    ];

    [Obsolete("This will be removed in a future release. Instead use 'RecipePermissions.ManageRecipes'.")]
    public static readonly Permission ManageReCaptchaSettings = new("ManageReCaptchaSettings", "Manage ReCaptcha Settings");

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        },
    ];
}
