using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Security;
using OrchardCore.Security.Services;

namespace OrchardCore.Liquid.DataMigrations;

internal sealed class PermissionMigrations : DataMigration
{
    private const string EditorDescription = "Grants users the ability to edit content and Liquid templates.";

    private readonly ShellSettings _shellSettings;

    public PermissionMigrations(ShellSettings shellSettings)
    {
        _shellSettings = shellSettings;
    }

    public int Create()
    {
        if (!_shellSettings.IsInitializing())
        {
            ShellScope.AddDeferredTask(GrantEditorPermissionAsync);
        }

        return 1;
    }

    private static async Task GrantEditorPermissionAsync(ShellScope shellScope)
    {
        var roleService = shellScope.ServiceProvider.GetRequiredService<IRoleService>();
        var roleStore = shellScope.ServiceProvider.GetRequiredService<IRoleStore<IRole>>();
        var editor = (await roleService.GetRolesAsync())
            .OfType<Role>()
            .FirstOrDefault(role => string.Equals(
                role.RoleName,
                OrchardCoreConstants.Roles.Editor,
                StringComparison.OrdinalIgnoreCase));

        if (editor is null)
        {
            return;
        }

        var changed = false;

        if (!editor.RoleClaims.Any(claim =>
            string.Equals(claim.ClaimType, OrchardCore.Security.Permissions.Permission.ClaimType, StringComparison.Ordinal) &&
            string.Equals(claim.ClaimValue, Permissions.ManageLiquidTemplates.Name, StringComparison.OrdinalIgnoreCase)))
        {
            editor.RoleClaims.Add(RoleClaim.Create(Permissions.ManageLiquidTemplates.Name));
            changed = true;
        }

        if (!string.Equals(editor.RoleDescription, EditorDescription, StringComparison.Ordinal))
        {
            editor.RoleDescription = EditorDescription;
            changed = true;
        }

        if (!changed)
        {
            return;
        }

        await roleStore.UpdateAsync(editor, CancellationToken.None);
    }
}
