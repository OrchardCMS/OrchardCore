using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
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
    private readonly AdminOptions _adminOptions;

    public RobotsSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IStaticFileProvider staticFileProvider,
        IOptions<AdminOptions> adminOptions)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _staticFileProvider = staticFileProvider;
        _adminOptions = adminOptions.Value;
    }

    public override async Task<IDisplayResult> EditAsync(RobotsSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, SeoPermissions.ManageSettings))
        {
            return null;
        }

        return Initialize<RobotsSettingsViewModel>("RobotsSettings_Edit", async model =>
        {
            var fileInfo = _staticFileProvider.GetFileInfo(RobotsMiddleware.RobotsFileName);

            model.PhysicalFileExists = fileInfo.Exists;

            if (String.IsNullOrEmpty(settings.FileContent))
            {
                if (fileInfo.Exists)
                {
                    using var stream = fileInfo.CreateReadStream();
                    using var reader = new StreamReader(stream);

                    model.FileContent = await reader.ReadToEndAsync();
                }
                else
                {
                    model.FileContent = SeoHelpers.GetDefaultRobotsContents(_adminOptions);
                }
            }
            else
            {
                model.FileContent = settings.FileContent;
            }
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
