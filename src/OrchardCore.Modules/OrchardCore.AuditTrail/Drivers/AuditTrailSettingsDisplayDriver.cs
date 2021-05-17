using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.AuditTrail.Permissions;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
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
        public const string AuditTrailSettingsGroupId = "AuditTrail";

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

        public override async Task<IDisplayResult> EditAsync(AuditTrailSettings settings, BuildEditorContext context) =>
            !await IsAuthorizedToManageAuditTrailSettingsAsync()
                ? null
                : Initialize<AuditTrailSettingsViewModel>("AuditTrailSettings_Edit", model =>
                {
                    var categoryDescriptors = _auditTrailManager.DescribeCategories();
                    var eventsSettings = settings.Categories.SelectMany(category => category.Events).ToList();

                    var categoriesSettingsViewModels = categoryDescriptors
                        .Select(category => new AuditTrailCategorySettingsViewModel()
                        {
                            Name = category.Name,
                            LocalizedName = category.LocalizedName,
                            Events = category.Events
                            .Select(@event =>
                            {
                                var eventSettings = GetOrCreate(eventsSettings, @event);

                                return new AuditTrailEventSettingsViewModel()
                                {
                                    Name = @event.Name,
                                    Category = @event.Category,
                                    LocalizedName = @event.LocalizedName,
                                    Description = @event.Description,
                                    IsEnabled = @event.IsMandatory || eventSettings.IsEnabled,
                                    IsMandatory = @event.IsMandatory
                                };
                            })
                            .ToArray()
                        })
                    .ToArray();

                    model.Categories = categoriesSettingsViewModels;
                    model.AllowedContentTypes = settings.AllowedContentTypes;
                    model.ClientIpAddressAllowed = settings.ClientIpAddressAllowed;
                }).Location("Content:1").OnGroup(AuditTrailSettingsGroupId);

        public override async Task<IDisplayResult> UpdateAsync(AuditTrailSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == AuditTrailSettingsGroupId)
            {
                if (!await IsAuthorizedToManageAuditTrailSettingsAsync())
                {
                    return null;
                }

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

                settings.AllowedContentTypes = model.AllowedContentTypes;
                settings.ClientIpAddressAllowed = model.ClientIpAddressAllowed;
            }

            return await EditAsync(settings, context);
        }

        private Task<bool> IsAuthorizedToManageAuditTrailSettingsAsync() =>
             _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, AuditTrailPermissions.ManageAuditTrailSettings);

        /// <summary>
        /// We're creating settings on the fly so that when the user updates the settings the first time,
        /// we won't log a massive amount of event settings that have changed.
        /// </summary>
        private static AuditTrailEventSettings GetOrCreate(IList<AuditTrailEventSettings> eventsSettings, AuditTrailEventDescriptor descriptor)
        {
            var settings = eventsSettings.FirstOrDefault(settings => settings.Category == descriptor.Category && settings.Name == descriptor.Name);
            if (settings == null)
            {
                settings = new AuditTrailEventSettings
                {
                    Name = descriptor.Name,
                    Category = descriptor.Category,
                    IsEnabled = descriptor.IsMandatory || descriptor.IsEnabledByDefault
                };

                eventsSettings.Add(settings);
            }

            return settings;
        }
    }
}
