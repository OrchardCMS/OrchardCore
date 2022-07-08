using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Security.Services;

namespace OrchardCore.ContentFields.Settings
{
    public class UserPickerFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<UserPickerField>
    {
        private readonly IRoleService _roleService;

        public UserPickerFieldSettingsDriver(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<UserPickerFieldSettingsViewModel>("UserPickerFieldSettings_Edit", async model =>
            {
                var settings = partFieldDefinition.GetSettings<UserPickerFieldSettings>();
                model.Hint = settings.Hint;
                model.Required = settings.Required;
                model.Multiple = settings.Multiple;
                var roles = (await _roleService.GetRoleNamesAsync())
                    .Except(new[] { "Anonymous", "Authenticated" }, StringComparer.OrdinalIgnoreCase)
                    .Select(roleName => new RoleEntry
                    {
                        Role = roleName,
                        IsSelected = settings.DisplayedRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase)
                    })
                    .ToArray();

                model.Roles = roles;
                model.DisplayAllUsers = settings.DisplayAllUsers || !roles.Where(x => x.IsSelected).Any();

            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new UserPickerFieldSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                var settings = new UserPickerFieldSettings
                {
                    Hint = model.Hint,
                    Required = model.Required,
                    Multiple = model.Multiple
                };

                var selectedRoles = model.Roles.Where(x => x.IsSelected).Select(x => x.Role).ToArray();

                if (model.DisplayAllUsers || selectedRoles.Length == 0)
                {
                    // No selected role should have the same effect as display all users
                    settings.DisplayedRoles = Array.Empty<string>();
                    settings.DisplayAllUsers = true;
                }
                else
                {
                    settings.DisplayedRoles = selectedRoles;
                    settings.DisplayAllUsers = false;
                }

                context.Builder.WithSettings(settings);
            }

            return Edit(partFieldDefinition, context.Updater);
        }
    }
}
