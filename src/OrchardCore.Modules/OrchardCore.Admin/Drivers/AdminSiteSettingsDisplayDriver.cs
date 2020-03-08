/*
    defines the class AdminSiteSettingsDisplayDriver (<- SectionDisplayDriver <ISite, AdminSettings>)
        allows you to change the model (.EditAsync) or update the settings (.UpdateAsync)
            returning the task with a display result
*/

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.Admin.Drivers
{
    public class AdminSiteSettingsDisplayDriver: SectionDisplayDriver <ISite, AdminSettings>
    {
        // (???) id in the user group OR group id
        public const string GroupId = "admin";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        // constructor
        public AdminSiteSettingsDisplayDriver (
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        /*
            returns a task with a display result
        */
        public override async Task <IDisplayResult> EditAsync (AdminSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext? .User;
            // if "user" does not have administrator privileges, then returns null
            if (! await _authorizationService.AuthorizeAsync (user, PermissionsAdminSettings.ManageAdminSettings))
            {
                return null;
            }

            /*
                creates and returns a "ShapeResult" object with arguments $model and $GroupId
            */
            return Initialize <AdminSettingsViewModel> ("AdminSettings_Edit",
                model =>
                // initialize $model to $settings
                {
                    model.DisplayMenuFilter = settings.DisplayMenuFilter;
                })
                .Location ("Content: 3")
                .OnGroup (GroupId);
        }

        /*
            returns a task with a display result
        */
        public override async Task <IDisplayResult> UpdateAsync (AdminSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext? .User;

            // if user does not have administrator privileges, then returns null
            if (! await _authorizationService.AuthorizeAsync (user, PermissionsAdminSettings.ManageAdminSettings))
            {
                return null;
            }

            // for administrators
            if (context.GroupId == GroupId)
            {
                var model = new AdminSettingsViewModel ();

                // tries to update the $context by $model
                await context.Updater.TryUpdateModelAsync (model, Prefix);

                // update the settings by $model
                settings.DisplayMenuFilter = model.DisplayMenuFilter;
            }

            // creates and returns an task with a display result
            return await EditAsync (settings, context);
        }
    }
}
