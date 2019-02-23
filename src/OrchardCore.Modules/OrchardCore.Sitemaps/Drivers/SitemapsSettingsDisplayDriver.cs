using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps
{
    public class SitemapsSettingsDisplayDriver : SectionDisplayDriver<ISite, SitemapsSettings>
    {
        public const string GroupId = "sitemaps";

        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SitemapsSettingsDisplayDriver(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<IDisplayResult> EditAsync(SitemapsSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageSitemaps))
            {
                return null;
            }

            return Initialize<SitemapsSettingsViewModel>("SitemapsSettings_Edit", model =>
            {
                model.MaxEntriesPerSitemap = settings.MaxEntriesPerSitemap != default ? settings.MaxEntriesPerSitemap : 50000;
            }).Location("Content:5").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(SitemapsSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageSitemaps))
            {
                return null;
            }

            if (context.GroupId == GroupId)
            {
                var model = new SitemapsSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.MaxEntriesPerSitemap = model.MaxEntriesPerSitemap;
            }

            return await EditAsync(settings, context);
        }
    }
}
