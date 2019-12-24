using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.Cors.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Settings;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Cors.Settings;
using OrchardCore.Entities;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;

namespace OrchardCore.Cors.Controllers
{
    [Admin]
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;

        private readonly IStringLocalizer T;
        private readonly IHtmlLocalizer<AdminController> TH;

        public AdminController(
            IAuthorizationService authorizationService,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            ISiteService siteService,
            INotifier notifier
            )
        {
            TH = htmlLocalizer;
            _notifier = notifier;
            _siteService = siteService;
            T = stringLocalizer;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageCorsSettings))
            {
                return Unauthorized();
            }

            var settings = (await _siteService.GetSiteSettingsAsync()).As<CorsSettings>();

            var list = new List<CorsPolicyViewModel>();

            if (settings?.Policies != null)
            {
                foreach (var policySetting in settings.Policies)
                {
                    var policyViewModel = new CorsPolicyViewModel()
                    {
                        Name = policySetting.Name,
                        AllowAnyHeader = policySetting.AllowAnyHeader,
                        AllowedHeaders = policySetting.AllowedHeaders,
                        AllowAnyMethod = policySetting.AllowAnyMethod,
                        AllowedMethods = policySetting.AllowedMethods,
                        AllowAnyOrigin = policySetting.AllowAnyOrigin,
                        AllowedOrigins = policySetting.AllowedOrigins,
                        AllowCredentials = policySetting.AllowCredentials
                    };

                    list.Add(policyViewModel);
                }
            }

            var viewModel = new CorsSettingsViewModel
            {
                Policies = list.ToArray()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ActionName(nameof(Index))]
        public async Task<ActionResult> IndexPOST()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageCorsSettings))
            {
                return Unauthorized();
            }

            var model = new CorsSettingsViewModel();
            var configJson = Request.Form["CorsSettings"].First();
            model.Policies = JsonConvert.DeserializeObject<CorsPolicyViewModel[]>(configJson);

            var corsPolicies = new List<CorsPolicySetting>();

            foreach (var settingViewModel in model.Policies)
            {
                corsPolicies.Add(new CorsPolicySetting
                {
                    Name = settingViewModel.Name,
                    AllowAnyHeader = settingViewModel.AllowAnyHeader,
                    AllowAnyMethod = settingViewModel.AllowAnyMethod,
                    AllowAnyOrigin = settingViewModel.AllowAnyOrigin,
                    AllowCredentials = settingViewModel.AllowCredentials,
                    AllowedHeaders = settingViewModel.AllowedHeaders,
                    AllowedMethods = settingViewModel.AllowedMethods,
                    AllowedOrigins = settingViewModel.AllowedOrigins
                });
            }

            var corsSettings = new CorsSettings()
            {
                Policies = corsPolicies
            };

            var siteSettings = await _siteService.LoadSiteSettingsAsync();
            siteSettings.Properties[nameof(CorsSettings)] = JObject.FromObject(corsSettings);
            await _siteService.UpdateSiteSettingsAsync(siteSettings);

            _notifier.Success(TH["Cors settings are updated"]);

            return View(model);
        }
    }
}
