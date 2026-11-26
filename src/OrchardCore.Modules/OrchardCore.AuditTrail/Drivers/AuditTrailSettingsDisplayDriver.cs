using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.AuditTrail.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.AuditTrail.Drivers;

public sealed class AuditTrailSettingsDisplayDriver : SiteDisplayDriver<AuditTrailSettings>
{
    private readonly IAuditTrailManager _auditTrailManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IServiceProvider _serviceProvider;

    internal readonly IStringLocalizer S;

    public AuditTrailSettingsDisplayDriver(
        IAuditTrailManager auditTrailManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IServiceProvider serviceProvider,
        IStringLocalizer<AuditTrailSettingsDisplayDriver> stringLocalizer)
    {
        _auditTrailManager = auditTrailManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _serviceProvider = serviceProvider;
        S = stringLocalizer;
    }

    protected override string SettingsGroupId
        => AuditTrailSettingsGroup.Id;

    public override async Task<IDisplayResult> EditAsync(ISite site, AuditTrailSettings settings, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, AuditTrailPermissions.ManageAuditTrailSettings))
        {
            return null;
        }

        return Combine(
            Initialize<AuditTrailSettingsViewModel>("AuditTrailSettings_Edit", model => PopulateViewModel(settings, model))
                .Location(l => l.Zone("Content", "1").Tab("Events").Card(S["Events"], "10"))
                .OnGroup(SettingsGroupId),
            Initialize<AuditTrailSettingsViewModel>("AuditTrailSettingsPrivacy_Edit", model => model.ClientIpAddressAllowed = settings.ClientIpAddressAllowed)
                .Location(l => l.Zone("Content", "2").Tab("Events").Card(S["Privacy"], "20"))
                .OnGroup(SettingsGroupId));
    }

    private void PopulateViewModel(AuditTrailSettings settings, AuditTrailSettingsViewModel model)
    {
        var categories = _auditTrailManager.DescribeCategories();

        var settingsGroups = settings.Categories
            .ToLookup(category => category.Events
            .FirstOrDefault()?.Category ?? "");

        var categoriesViewModel = categories
            .Select(category => new AuditTrailCategorySettingsViewModel()
            {
                Name = category.Name,
                LocalizedName = category.LocalizedName(_serviceProvider),
                Events = category.Events.Values
                    .Select(auditTrailEvent =>
                    {
                        var settings = settingsGroups[auditTrailEvent.Category]
                            .FirstOrDefault()?.Events
                            .FirstOrDefault(settings => settings.Name == auditTrailEvent.Name);

                        return new AuditTrailEventSettingsViewModel()
                        {
                            Name = auditTrailEvent.Name,
                            Category = auditTrailEvent.Category,
                            LocalizedName = auditTrailEvent.LocalizedName(_serviceProvider),
                            Description = auditTrailEvent.Description(_serviceProvider),
                            IsEnabled = auditTrailEvent.IsMandatory || (settings?.IsEnabled ?? auditTrailEvent.IsEnabledByDefault),
                            IsMandatory = auditTrailEvent.IsMandatory,
                        };
                    }).ToArray(),
            }).ToArray();

        model.Categories = categoriesViewModel;
        model.ClientIpAddressAllowed = settings.ClientIpAddressAllowed;
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, AuditTrailSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, AuditTrailPermissions.ManageAuditTrailSettings))
        {
            return null;
        }

        if (context.GroupId == AuditTrailSettingsGroup.Id)
        {
            var model = new AuditTrailSettingsViewModel();
            await context.Updater.TryUpdateModelAsync(model, Prefix);

            settings.Categories = model.Categories
                .Select(categorySettings => new AuditTrailCategorySettings()
                {
                    Name = categorySettings.Name,
                    Events = categorySettings.Events
                        .Select(settings => new AuditTrailEventSettings()
                        {
                            Name = settings.Name,
                            Category = settings.Category,
                            IsEnabled = settings.IsEnabled,
                        })
                        .ToArray(),
                })
                .ToArray();

            settings.ClientIpAddressAllowed = model.ClientIpAddressAllowed;
        }

        return await EditAsync(site, settings, context);
    }
}
