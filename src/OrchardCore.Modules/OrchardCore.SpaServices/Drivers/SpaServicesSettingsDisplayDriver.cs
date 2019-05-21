using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.SpaServices.Settings;
using OrchardCore.SpaServices.ViewModels;

namespace OrchardCore.SpaServices.Drivers
{
    public class SpaServicesSettingsDisplayDriver : SectionDisplayDriver<ISite, SpaServicesSettings>
    {
        public const string GroupId = "SpaServicesSettings";

        private readonly ISiteService _siteService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;

        public SpaServicesSettingsDisplayDriver(ISiteService siteService,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor,
            IShellHost shellHost,
            ShellSettings shellSettings)
        {
            _siteService = siteService;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
        }

        public override IDisplayResult Edit(SpaServicesSettings settings, BuildEditorContext context)
        {
            return Initialize<SpaServicesSettingsViewModel>("SpaServicesSettings_Edit", async model =>
            {
                model.IsHomepage = false;
                model.SetHomepage = false;

                var site = await _siteService.GetSiteSettingsAsync();
                var homeRoute = site.HomeRoute;

                model.UseStaticFile = settings.UseStaticFile;
                model.StaticFile = settings.StaticFile;

                if (homeRoute["area"]?.ToString() == "OrchardCore.SpaServices" &&
                    homeRoute["controller"]?.ToString() == "Home" &&
                    homeRoute["action"]?.ToString() == "Index")
                {
                    model.IsHomepage = true;
                }
            }).Location("Content:5").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(ISite model, SpaServicesSettings section, IUpdateModel updater, BuildEditorContext context)
        {
            if (context.GroupId == GroupId)
            {
                var viewModel = new SpaServicesSettingsViewModel();
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null && await _authorizationService.AuthorizeAsync(httpContext.User, Permissions.SetHomepage))
                {
                    if (await updater.TryUpdateModelAsync(viewModel, Prefix))
                    {
                        // If the settings are valid, reload the current tenant.
                        if (context.Updater.ModelState.IsValid)
                        {
                            await updater.TryUpdateModelAsync(section, Prefix, t => t.StaticFile, t => t.UseStaticFile);

                            if (viewModel.SetHomepage)
                            {
                                var homeRoute = model.HomeRoute;
                                homeRoute["area"] = "OrchardCore.SpaServices";
                                homeRoute["controller"] = "Home";
                                homeRoute["action"] = "Index";
                            }

                            await _shellHost.ReloadShellContextAsync(_shellSettings);
                        }

                    }
                }
            }
            return await EditAsync(section, context);
        }
    }
}
