using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Media.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Admin.Drivers
{
    public class AdminSiteSettingsDisplayDriver : SectionDisplayDriver<ISite, AdminSettings>
    {
        public const string GroupId = "admin";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public AdminSiteSettingsDisplayDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override async Task<IDisplayResult> EditAsync(AdminSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, PermissionsAdminSettings.ManageAdminSettings))
            {
                return null;
            }

            var brandImagePaths = new List<EditMediaFieldItemInfo> { };
            brandImagePaths.Add(new EditMediaFieldItemInfo { Path = settings.BrandImage });
            var faviconImagePaths = new List<EditMediaFieldItemInfo> { };
            faviconImagePaths.Add(new EditMediaFieldItemInfo { Path = settings.Favicon });

            return Initialize<AdminSettingsViewModel>("AdminSettings_Edit", model =>
                {
                    model.DisplayMenuFilter = settings.DisplayMenuFilter;
                    model.BrandImage = JsonConvert.SerializeObject(brandImagePaths);
                    model.Favicon = JsonConvert.SerializeObject(faviconImagePaths);
                    model.Head = settings.Head;
                }).Location("Content:3").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(AdminSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, PermissionsAdminSettings.ManageAdminSettings))
            {
                return null;
            }

            if (context.GroupId == GroupId)
            {
                var model = new AdminSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.DisplayMenuFilter = model.DisplayMenuFilter;

                // Deserializing an empty string doesn't return an array
                var BrandImageItems = String.IsNullOrWhiteSpace(model.BrandImage)
                    ? new List<EditMediaFieldItemInfo>()
                    : JsonConvert.DeserializeObject<EditMediaFieldItemInfo[]>(model.BrandImage).ToList();

                if (BrandImageItems.Count > 0)
                {
                    settings.BrandImage = BrandImageItems.FirstOrDefault().Path;
                }

                // Deserializing an empty string doesn't return an array
                var FaviconItems = String.IsNullOrWhiteSpace(model.Favicon)
                    ? new List<EditMediaFieldItemInfo>()
                    : JsonConvert.DeserializeObject<EditMediaFieldItemInfo[]>(model.Favicon).ToList();

                if (FaviconItems.Count > 0)
                {
                    settings.Favicon = FaviconItems.FirstOrDefault().Path;
                }

                settings.Head = model.Head;
            }

            return await EditAsync(settings, context);
        }
    }
}
