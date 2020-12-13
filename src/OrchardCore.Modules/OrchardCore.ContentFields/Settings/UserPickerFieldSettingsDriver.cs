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
                model.DisplayAllUsers = settings.DisplayAllUsers;
                model.Roles = (await _roleService.GetRoleNamesAsync())
                    .Except(new[] { "Anonymous", "Authenticated" }, StringComparer.OrdinalIgnoreCase)
                    .Select(x => new RoleEntry { Role = x, IsSelected = settings.DisplayedRoles.Contains(x, StringComparer.OrdinalIgnoreCase) }).ToArray();
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new UserPickerFieldSettingsViewModel();
            var settings = new UserPickerFieldSettings();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                settings.Hint= model.Hint;
                settings.Required = model.Required;
                settings.Multiple = model.Multiple;
                settings.DisplayAllUsers = model.DisplayAllUsers;
                if (settings.DisplayAllUsers)
                {
                    settings.DisplayedRoles = Array.Empty<String>();
                }
                else
                {
                    settings.DisplayedRoles = model.Roles.Where(x => x.IsSelected).Select(x => x.Role).ToArray();
                }

                context.Builder.WithSettings(settings);
            }

            return Edit(partFieldDefinition);
        }
    }
}
