using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules.FileProviders;
using OrchardCore.Seo.Services;
using OrchardCore.Seo.Settings;
using OrchardCore.Seo.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Seo.Drivers;

public class RobotsSettingsDisplayDriver : SectionDisplayDriver<ISite, RobotsSettings>
{
    public const string GroupId = "robotsSettings";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IStaticFileProvider _staticFileProvider;

    public RobotsSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IStaticFileProvider staticFileProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _staticFileProvider = staticFileProvider;
    }

    public override async Task<IDisplayResult> EditAsync(RobotsSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, SeoPermissions.ManageSettings))
        {
            return null;
        }

        return Initialize<RobotsSettingsViewModel>("RobotsSettings_Edit", model =>
        {
            var fileInfo = _staticFileProvider.GetFileInfo(RobotsMiddleware.RobotsFileName);

            model.PhysicalFileExists = fileInfo.Exists;
            model.FileContent = settings.FileContent;
        }).Location("Content:3")
        .OnGroup(GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(RobotsSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!context.GroupId.Equals(GroupId, StringComparison.OrdinalIgnoreCase)
            || !await _authorizationService.AuthorizeAsync(user, SeoPermissions.ManageSettings))
        {
            return null;
        }

        var model = new RobotsSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.FileContent = model.FileContent;

        return await EditAsync(settings, context);
    }
}
