using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.Cors.Services;
using OrchardCore.Cors.Settings;
using OrchardCore.Cors.ViewModels;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Cors.Controllers;

[Admin]
public sealed class AdminController : Controller
{
    private readonly IShellReleaseManager _releaseManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly CorsService _corsService;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;

    public AdminController(
        IShellReleaseManager releaseManager,
        IAuthorizationService authorizationService,
        CorsService corsService,
        INotifier notifier,
        IHtmlLocalizer<AdminController> htmlLocalizer
        )
    {
        _releaseManager = releaseManager;
        _authorizationService = authorizationService;
        _corsService = corsService;
        _notifier = notifier;
        H = htmlLocalizer;
    }

    [HttpGet]
    [Admin("Cors", "CorsIndex")]
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
                    IsDefaultPolicy = policySetting.IsDefaultPolicy,
                    ExposedHeaders = policySetting.ExposedHeaders,
                };

                list.Add(policyViewModel);
            }
        }

        var viewModel = new CorsSettingsViewModel
        {
            Policies = list.ToArray(),
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
        var configJson = Request.Form["CorsSettings"].FirstOrDefault();
        try
        {
            model.Policies = string.IsNullOrWhiteSpace(configJson) ? null : JConvert.DeserializeObject<CorsPolicyViewModel[]>(configJson);
        }
        catch (JsonException)
        {
            ModelState.AddModelError(string.Empty, H["Provide a valid CORS policies array."].Value);
        }
        if (model.Policies is null || !ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, H["Provide a CORS policies array; use an empty array to remove policies."].Value);
            model.Policies ??= [];
            return View(model);
        }

        var corsPolicies = new List<CorsPolicySetting>();

        foreach (var settingViewModel in model.Policies ?? [])
        {
            corsPolicies.Add(settingViewModel is null ? null : new CorsPolicySetting
            {
                Name = settingViewModel.Name,
                AllowAnyHeader = settingViewModel.AllowAnyHeader,
                AllowAnyMethod = settingViewModel.AllowAnyMethod,
                AllowAnyOrigin = settingViewModel.AllowAnyOrigin,
                AllowCredentials = settingViewModel.AllowCredentials,
                AllowedHeaders = settingViewModel.AllowedHeaders,
                AllowedMethods = settingViewModel.AllowedMethods,
                AllowedOrigins = settingViewModel.AllowedOrigins,
                IsDefaultPolicy = settingViewModel.IsDefaultPolicy,
                ExposedHeaders = settingViewModel.ExposedHeaders,
            });
        }

        var corsSettings = new CorsSettings()
        {
            Policies = corsPolicies,
        };

        var result = await _corsService.UpdateSettingsAsync(corsSettings);
        if (result.Errors.Count > 0)
        {
            foreach (var error in result.Errors.Values.SelectMany(messages => messages))
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }
        if (result.Changed)
        {
            _releaseManager.RequestRelease();
        }

        await _notifier.SuccessAsync(H["The CORS settings have updated successfully."]);

        return View(model);
    }

}
