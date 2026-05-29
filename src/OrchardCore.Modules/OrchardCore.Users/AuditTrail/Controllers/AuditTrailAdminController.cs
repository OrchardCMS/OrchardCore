using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Compliance.Redaction;
using OrchardCore.Admin;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Users.AuditTrail.Handlers;
using OrchardCore.Users.AuditTrail.Models;
using OrchardCore.Users.AuditTrail.Services;
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
    private readonly IEnumerable<Redactor> _redactors;

    public AuditTrailAdminController(
        IAuthorizationService authorizationService,
        ISiteService siteService,
        IEnumerable<CustomUserSettingsService> customUserSettingsServices,
        IEnumerable<Redactor> redactors)
    {
        _authorizationService = authorizationService;
        _siteService = siteService;
        _customUserSettingsService = customUserSettingsServices.FirstOrDefault();
        _redactors = redactors.Where(redactor => redactor is not null);
    }

    public async Task<ActionResult> Index()
    {
        var settings = await _siteService.GetSettingsAsync<AuditTrailUserEventSettings>();
        var redactorSettings = settings.UserSnapshotRedactors ?? new Dictionary<string, string>();
        var redactors = _redactors.ToDictionary(item => item.GetType().Name);

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
                    Redactor = redactorSettings.TryGetValue(name, out var value) && redactors.ContainsKey(value)
                        ? value
                        : nameof(RemoveRedactor),
                })
                .ToArray(),
            RedactorNames = redactors.Keys,
        });
    }

    [HttpPost, ActionName(nameof(Index))]
    public async Task<ActionResult> IndexFilterPOST([FromForm] AuditTrailUserEventSettingsViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageUserAuditTrailSettings))
        {
            return Forbid();
        }

        var selected = model
            .UserSnapshotProperties
            .Where(item => item.Redactor != nameof(RemoveRedactor))
            .ToDictionary(item => item.Name, item => item.Redactor);

        var siteSettings = await _siteService.LoadSiteSettingsAsync();
        var settings = siteSettings.GetOrCreate<AuditTrailUserEventSettings>();
        settings.UserSnapshotRedactors = selected;
        siteSettings.Put(nameof(AuditTrailUserEventSettings), settings);
        await _siteService.UpdateSiteSettingsAsync(siteSettings);

        return RedirectToAction(nameof(Index));
    }
}
