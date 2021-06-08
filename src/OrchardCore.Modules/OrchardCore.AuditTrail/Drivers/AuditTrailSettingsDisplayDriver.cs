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

        public AuditTrailSettingsDisplayDriver(
            IAuditTrailManager auditTrailManager,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _auditTrailManager = auditTrailManager;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override async Task<IDisplayResult> EditAsync(AuditTrailSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (!await _authorizationService.AuthorizeAsync(user, AuditTrailPermissions.ManageAuditTrailSettings))
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
                        LocalizedName = category.LocalizedName,
                        Events = category.Events
                            .Select(auditTrailEvent =>
                            {
                                var settings = settingsGroups[auditTrailEvent.Category]
                                    .FirstOrDefault()?.Events
                                    .FirstOrDefault(settings => settings.Name == auditTrailEvent.Name);

                                return new AuditTrailEventSettingsViewModel()
                                {
                                    Name = auditTrailEvent.Name,
                                    Category = auditTrailEvent.Category,
                                    LocalizedName = auditTrailEvent.LocalizedName,
                                    Description = auditTrailEvent.Description,
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
            var user = _httpContextAccessor.HttpContext?.User;
            if (!await _authorizationService.AuthorizeAsync(user, AuditTrailPermissions.ManageAuditTrailSettings))
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
                        LocalizedName = categorySettings.LocalizedName,
                        Events = categorySettings.Events
                            .Select(settings => new AuditTrailEventSettings()
                            {
                                Name = settings.Name,
                                LocalizedName = settings.LocalizedName,
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
