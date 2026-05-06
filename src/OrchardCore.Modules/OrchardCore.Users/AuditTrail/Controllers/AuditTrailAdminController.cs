using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.Settings;
using OrchardCore.Users.AuditTrail.Handlers;
using OrchardCore.Users.AuditTrail.Models;
using OrchardCore.Users.AuditTrail.ViewModels;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OrchardCore.Users.AuditTrail.Controllers;

[Admin("Users/AuditTrail/{action}/{id?}")]
public sealed class AuditTrailAdminController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ISiteService _siteService;
    private readonly CustomUserSettingsService _customUserSettingsService;

    public AuditTrailAdminController(
        IAuthorizationService authorizationService,
        ISiteService siteService,
        IEnumerable<CustomUserSettingsService> customUserSettingsServices)
    {
        _authorizationService = authorizationService;
        _siteService = siteService;
        _customUserSettingsService = customUserSettingsServices.FirstOrDefault();
    }

    public async Task<ActionResult> Index()
    {
        var settings = await _siteService.GetSettingsAsync<AuditTrailUserEventSettings>();
        var selected = (settings.UserSnapshotProperties ?? []).ToHashSet();

        var propertyNames = JsonSerializer.SerializeToNode(new User())
            .ToObject<IDictionary<string, object>>()
            .Keys
            .Union(await _customUserSettingsService.GetAllSettingsTypeNamesAsync())
            .Where(name => !UserEventHandler.BannedProperties.Contains(name))
            .Order();
        
        return View(new AuditTrailUserEventSettingsViewModel
        {
            UserSnapshotProperties = propertyNames
                .Select(name => new AuditTrailUserEventSettingsViewModel.UserSnapshotPropertiesEntry
                {
                    Name = name,
                    Selected = selected.Contains(name),
                })
                .ToArray(),
        });
    }

    [HttpPost, ActionName(nameof(Index))]
    public async Task<ActionResult> IndexFilterPOST([FromForm] AuditTrailUserEventSettingsViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageUserAuditTrailSettings))
        {
            return Forbid();
        }

        // UserId is always included, to ensure that the account is identifiable within the system just using the data
        // in the snapshot. This is not PII, so storing it long term is not problematic.
        var selected = model
            .UserSnapshotProperties
            .Where(item => item.Selected)
            .Select(item => item.Name)
            .Union([nameof(Users.Models.User.UserId)]);

        var siteSettings = await _siteService.LoadSiteSettingsAsync();
        var settings = siteSettings.GetOrCreate<AuditTrailUserEventSettings>();
        settings.UserSnapshotProperties = selected;
        siteSettings.Properties[nameof(AuditTrailUserEventSettings)] = JObject.FromObject(settings);
        await _siteService.UpdateSiteSettingsAsync(siteSettings);

        return RedirectToAction(nameof(Index));
    }
}
