using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.ViewModels;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Security.Services;

namespace OrchardCore.Lists.Settings
{
    public class RoleCommonPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<CommonPart>
    {
        private readonly IStringLocalizer S;
        private readonly IRoleService _roleService;

        public RoleCommonPartSettingsDisplayDriver(
            IStringLocalizer<RoleCommonPartSettingsDisplayDriver> stringLocalizer,
            IRoleService roleService)
        {
            S = stringLocalizer;
            _roleService = roleService;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            return Initialize<RoleCommonPartSettingsViewModel>("RoleCommonPartSettings_Edit", async model =>
            {
                var settings = contentTypePartDefinition.GetSettings<CommonPartSettings>();
                model.RoleItems = (await GetRoleNames()).Select(roleName => new RoleEntry()
                {
                    RoleName = roleName,
                    IsSelected = settings.Roles.Contains(roleName),
                }).ToList();
                model.SiteOwnerOnly = settings.Roles == null || settings.Roles.Length == 0;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            var model = new RoleCommonPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                var roles = Array.Empty<string>();

                if (!model.SiteOwnerOnly)
                {
                    var roleNames = await GetRoleNames();

                    var selectedRoles = model.RoleItems
                        ?.Where(roleEntry => roleEntry.IsSelected && roleNames.Contains(roleEntry.RoleName))
                        ?.ToList() ?? new List<RoleEntry>();

                    if (selectedRoles.Count == 0)
                    {
                        context.Updater.ModelState.AddModelError(Prefix, nameof(model.RoleItems), S["At least one role is required."]);
                    }
                    else
                    {
                        roles = selectedRoles.Select(selectedRole => selectedRole.RoleName).ToArray();
                    }
                }

                if (context.Updater.ModelState.IsValid)
                {
                    // CommonPartSettings could be set by another driver. Get existing settings first, then update it.
                    var settings = contentTypePartDefinition.GetSettings<CommonPartSettings>();
                    settings.Roles = roles;

                    context.Builder.WithSettings(settings);
                }
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }

        private async Task<IList<string>> GetRoleNames()
        {
            return (await _roleService.GetRoleNamesAsync()).Where(x => !RoleHelper.SystemRoleNames.Contains(x)).ToList();
        }
    }
}
