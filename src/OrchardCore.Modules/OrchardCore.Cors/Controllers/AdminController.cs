using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Newtonsoft.Json;
using OrchardCore.Admin;
using OrchardCore.Cors.Services;
using OrchardCore.Cors.Settings;
using OrchardCore.Cors.ViewModels;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Cors.Controllers
{
    [Admin]
    public class AdminController : Controller
    {
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IAuthorizationService _authorizationService;
        private readonly CorsService _corsService;
        private readonly INotifier _notifier;
        protected readonly IHtmlLocalizer H;

        public AdminController(
            IShellHost shellHost,
            ShellSettings shellSettings,
            IAuthorizationService authorizationService,
            CorsService corsService,
            INotifier notifier,
            IHtmlLocalizer<AdminController> htmlLocalizer
            )
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _authorizationService = authorizationService;
            _corsService = corsService;
            _notifier = notifier;
            H = htmlLocalizer;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageCorsSettings))
            {
                return Unauthorized();
            }

            var settings = await _corsService.GetSettingsAsync();

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
                        AllowCredentials = policySetting.AllowCredentials,
                        IsDefaultPolicy = policySetting.IsDefaultPolicy
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
                    AllowedOrigins = settingViewModel.AllowedOrigins,
                    IsDefaultPolicy = settingViewModel.IsDefaultPolicy

                });
            }

            var corsSettings = new CorsSettings()
            {
                Policies = corsPolicies
            };

            await _corsService.UpdateSettingsAsync(corsSettings);

            await _shellHost.ReleaseShellContextAsync(_shellSettings);

            await _notifier.SuccessAsync(H["The CORS settings have updated successfully."]);

            return View(model);
        }
    }
}
