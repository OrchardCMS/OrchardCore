using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.AuditTrail.Permissions;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.AuditTrail.ViewModels;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.AuditTrail.Drivers
{
    public class AuditTrailSettingsDisplayDriver : SectionDisplayDriver<ISite, AuditTrailSettings>
    {
        public const string AuditTrailSettingsGroupId = "AuditTrail";

        private readonly IHttpContextAccessor _hca;
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentDefinitionManager _contentDefitinionManager;


        public AuditTrailSettingsDisplayDriver(
            IHttpContextAccessor hca,
            IAuditTrailManager auditTrailManager,
            IAuthorizationService authorizationService,
            IContentDefinitionManager contentDefitinionManager)
        {
            _hca = hca;
            _auditTrailManager = auditTrailManager;
            _authorizationService = authorizationService;
            _contentDefitinionManager = contentDefitinionManager;
        }


        public override async Task<IDisplayResult> EditAsync(AuditTrailSettings settings, BuildEditorContext context) =>
            !await IsAuthorizedToManageAuditTrailSettingsAsync()
                ? null
                : Initialize<AuditTrailSettingsViewModel>($"{nameof(AuditTrailSettings)}_Edit", model =>
                {
                    var descriptors = _auditTrailManager.DescribeCategories();
                    var eventSettings = settings.EventSettings;

                    var categories = descriptors.GroupBy(categoryDescriptor => categoryDescriptor.Category)
                        .Select(categoryDescriptor =>
                        new
                        {
                            Category = categoryDescriptor.Key,
                            Name = categoryDescriptor.Select(categoryDescriptor => categoryDescriptor.LocalizedName),
                            Events = categoryDescriptor.SelectMany(categoryDescriptor => categoryDescriptor.Events)
                        })
                        .Select(categoryDescriptor =>
                            new AuditTrailCategorySettingsViewModel
                            {
                                Category = categoryDescriptor.Category,
                                Name = categoryDescriptor.Name.First(),
                                Events = categoryDescriptor.Events.Select(eventDescriptor =>
                                {
                                    var eventSetting = GetOrCreate(eventSettings.ToList(), eventDescriptor);

                                    return new AuditTrailEventSettingsViewModel
                                    {
                                        Event = eventDescriptor.FullEventName,
                                        Name = eventDescriptor.LocalizedName,
                                        Description = eventDescriptor.Description,
                                        IsEnabled = eventDescriptor.IsMandatory || eventSetting.IsEnabled,
                                        IsMandatory = eventDescriptor.IsMandatory
                                    };
                                }).ToList()
                            }).ToList();

                    model.Categories = categories;
                    model.EnableClientIpAddressLogging = settings.EnableClientIpAddressLogging;
                    model.BlacklistedContentTypes = _contentDefitinionManager.ListTypeDefinitions()
                        .Select(contentTypeDefiniton =>
                            new BlacklistedContentTypesViewModel
                            {
                                Name = contentTypeDefiniton.Name,
                                DisplayName = contentTypeDefiniton.DisplayName,
                                Selected = settings.BlacklistedContentTypeNames.Contains(contentTypeDefiniton.Name)
                            }).OrderBy(contentTypeDefinition => contentTypeDefinition.DisplayName).ToList();
                }).Location("Content:1").OnGroup(AuditTrailSettingsGroupId);

        public override async Task<IDisplayResult> UpdateAsync(AuditTrailSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == AuditTrailSettingsGroupId)
            {
                if (!await IsAuthorizedToManageAuditTrailSettingsAsync()) return null;

                var model = new AuditTrailSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                var eventSettings = model.Categories;
                var eventsDictionary = _auditTrailManager.DescribeProviders().Describe()
                    .SelectMany(categoryDescriptor => categoryDescriptor.Events)
                    .ToDictionary(eventDescriptor => eventDescriptor.FullEventName);

                foreach (var eventSettingViewModel in model.Categories.SelectMany(categorySettings => categorySettings.Events))
                {
                    var eventSetting = eventSettings
                        .SelectMany(categorySettings => categorySettings.Events
                            .Where(eventSettings => eventSettings.Event == eventSettingViewModel.Event))
                            .FirstOrDefault();

                    var descriptor = eventsDictionary[eventSetting.Event];
                    eventSetting.IsEnabled = eventSettingViewModel.IsEnabled || descriptor.IsMandatory;
                }

                settings.EnableClientIpAddressLogging = model.EnableClientIpAddressLogging;
                settings.BlacklistedContentTypes = model.BlacklistedContentTypes;

                settings.BlacklistedContentTypeNames = model.BlacklistedContentTypes
                    .Where(blacklistedContentType => blacklistedContentType.Selected)
                    .Select(blacklistedContentType => blacklistedContentType.Name).ToArray();

                settings.EventSettings = eventSettings
                    .SelectMany(categorySettings => categorySettings.Events
                        .Select(eventSettings =>
                            new AuditTrailEventSetting { EventName = eventSettings.Event, IsEnabled = eventSettings.IsEnabled }));
            }

            return await EditAsync(settings, context);
        }


        private Task<bool> IsAuthorizedToManageAuditTrailSettingsAsync() =>
             _authorizationService.AuthorizeAsync(_hca.HttpContext.User, AuditTrailPermissions.ManageAuditTrailSettings);

        /// <summary>
        /// We're creating settings on the fly so that when the user updates the settings the first time,
        /// we won't log a massive amount of event settings that have changed.
        /// </summary>
        private AuditTrailEventSetting GetOrCreate(IList<AuditTrailEventSetting> settings, AuditTrailEventDescriptor descriptor)
        {
            var setting = settings.FirstOrDefault(x => x.EventName == descriptor.FullEventName);

            if (setting == null)
            {
                setting = new AuditTrailEventSetting
                {
                    EventName = descriptor.FullEventName,
                    IsEnabled = descriptor.IsMandatory || descriptor.IsEnabledByDefault
                };

                settings.Add(setting);
            }

            return setting;
        }
    }
}
