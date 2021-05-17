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
                    var categories = _auditTrailManager.DescribeCategories();
                    var eventsSettings = settings.Events;

                    var categoriesSettings = categories.GroupBy(category => category.Name)
                        .Select(categories =>
                        new
                        {
                            Name = categories.Key,
                            LocalizedNames = categories.Select(category => category.LocalizedName),
                            Events = categories.SelectMany(category => category.Events)
                        })
                        .Select(category =>
                            new AuditTrailCategorySettingsViewModel
                            {
                                Name = category.Name,
                                LocalizedName = category.LocalizedNames.First(),
                                Events = category.Events.Select(descriptor =>
                                {
                                    var eventSettings = GetOrCreate(eventsSettings, descriptor);

                                    return new AuditTrailEventSettingsViewModel
                                    {
                                        FullName = descriptor.FullName,
                                        LocalizedName = descriptor.LocalizedName,
                                        Description = descriptor.Description,
                                        IsEnabled = descriptor.IsMandatory || eventSettings.IsEnabled,
                                        IsMandatory = descriptor.IsMandatory
                                    };
                                })
                                .ToArray()
                            })
                        .ToArray();

                    model.Categories = categoriesSettings;
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
                var categoriesSettings = model.Categories;

                var events = _auditTrailManager.DescribeProviders().Describe()
                    .SelectMany(category => category.Events)
                    .ToDictionary(@event => @event.FullName);

                foreach (var categorySettings in categoriesSettings)
                {
                    foreach (var eventSettings in categorySettings.Events)
                    {
                        var @event = events[eventSettings.FullName];
                        if (@event.IsMandatory)
                        {
                            eventSettings.IsEnabled = true;
                        }
                    }
                }

                settings.AllowedContentTypes = model.AllowedContentTypes;
                settings.ClientIpAddressAllowed = model.ClientIpAddressAllowed;

                settings.Events = categoriesSettings
                    .SelectMany(categorySettings => categorySettings.Events
                        .Select(eventSettings =>
                            new AuditTrailEventSettings
                            {
                                FullName = eventSettings.FullName,
                                IsEnabled = eventSettings.IsEnabled
                            }))
                    .ToArray();
            }

            return await EditAsync(settings, context);
        }

        private Task<bool> IsAuthorizedToManageAuditTrailSettingsAsync() =>
             _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, AuditTrailPermissions.ManageAuditTrailSettings);

        /// <summary>
        /// We're creating settings on the fly so that when the user updates the settings the first time,
        /// we won't log a massive amount of event settings that have changed.
        /// </summary>
        private static AuditTrailEventSettings GetOrCreate(IList<AuditTrailEventSettings> settings, AuditTrailEventDescriptor descriptor)
        {
            var setting = settings.FirstOrDefault(x => x.FullName == descriptor.FullName);
            if (setting == null)
            {
                setting = new AuditTrailEventSettings
                {
                    FullName = descriptor.FullName,
                    IsEnabled = descriptor.IsMandatory || descriptor.IsEnabledByDefault
                };

                settings.Add(setting);
            }

            return setting;
        }
    }
}
