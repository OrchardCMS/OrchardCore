using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.AuditTrail.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.AuditTrail.Drivers
{
    public class AuditTrailSettingsDisplayDriver : SectionDisplayDriver<ISite, AuditTrailSettings>
    {
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IServiceProvider _serviceProvider;

        public AuditTrailSettingsDisplayDriver(
            IAuditTrailManager auditTrailManager,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            IServiceProvider serviceProvider)
        {
            _auditTrailManager = auditTrailManager;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _serviceProvider = serviceProvider;
        }

        public override async Task<IDisplayResult> EditAsync(AuditTrailSettings settings, BuildEditorContext context)
        {
            if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, AuditTrailPermissions.ManageAuditTrailSettings))
            {
                return null;
            }

            return Initialize<AuditTrailSettingsViewModel>("AuditTrailSettings_Edit", model =>
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
                                    IsMandatory = auditTrailEvent.IsMandatory
                                };
                            })
                        .ToArray()
                    })
                .ToArray();

                model.Categories = categoriesViewModel;
                model.ClientIpAddressAllowed = settings.ClientIpAddressAllowed;
            }).Location("Content:1#Events").OnGroup(AuditTrailSettingsGroup.Id);
        }

        public override async Task<IDisplayResult> UpdateAsync(AuditTrailSettings settings, BuildEditorContext context)
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
                                IsEnabled = settings.IsEnabled
                            })
                            .ToArray()
                    })
                    .ToArray();

                settings.ClientIpAddressAllowed = model.ClientIpAddressAllowed;
            }

            return await EditAsync(settings, context);
        }
    }
}
