using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Seo;
using OrchardCore.Settings;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Drivers;

public class SitemapsRobotsSettingsDisplayDriver : SectionDisplayDriver<ISite, SitemapsRobotsSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public SitemapsRobotsSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    public override async Task<IDisplayResult> EditAsync(SitemapsRobotsSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, SeoConstants.ManageSeoSettings))
        {
            return null;
        }

        return Initialize<SitemapsRobotsSettings>("SitemapsRobotsSettings_Edit", model =>
        {
            model.IncludeSitemaps = settings.IncludeSitemaps;
        }).Location("Content:4")
        .OnGroup(SeoConstants.RobotsSettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(SitemapsRobotsSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!context.GroupId.Equals(SeoConstants.RobotsSettingsGroupId, StringComparison.OrdinalIgnoreCase)
            || !await _authorizationService.AuthorizeAsync(user, SeoConstants.ManageSeoSettings))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        return await EditAsync(settings, context);
    }
}
