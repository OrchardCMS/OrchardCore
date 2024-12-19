using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Security.Services;

namespace OrchardCore.ContentFields.Settings;

public sealed class UserPickerFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<UserPickerField>
{
    private readonly IRoleService _roleService;

    public UserPickerFieldSettingsDriver(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<UserPickerFieldSettingsViewModel>("UserPickerFieldSettings_Edit", async model =>
        {
            var settings = partFieldDefinition.GetSettings<UserPickerFieldSettings>();
            model.Hint = settings.Hint;
            model.Required = settings.Required;
            model.Multiple = settings.Multiple;
            var roles = await _roleService.GetAssignableRolesAsync();
            var roleEntries = roles.Select(role => new RoleEntry
            {
                Role = role.RoleName,
                IsSelected = settings.DisplayedRoles.Contains(role.RoleName, StringComparer.OrdinalIgnoreCase),
            }).ToArray();

            model.Roles = roleEntries;
            model.DisplayAllUsers = settings.DisplayAllUsers || !roleEntries.Where(x => x.IsSelected).Any();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        var model = new UserPickerFieldSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var settings = new UserPickerFieldSettings
        {
            Hint = model.Hint,
            Required = model.Required,
            Multiple = model.Multiple
        };

        var roles = await _roleService.GetAssignableRolesAsync();

        var selectedRoles = model.Roles
            .Where(x => x.IsSelected && roles.Any(y => y.RoleName == x.Role))
            .Select(x => x.Role)
            .ToArray();

        if (model.DisplayAllUsers || selectedRoles.Length == 0)
        {
            // No selected role should have the same effect as display all users
            settings.DisplayedRoles = [];
            settings.DisplayAllUsers = true;
        }
        else
        {
            settings.DisplayedRoles = selectedRoles;
            settings.DisplayAllUsers = false;
        }

        context.Builder.WithSettings(settings);

        return Edit(partFieldDefinition, context);
    }
}
