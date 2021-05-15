using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Abstractions.Setup;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Providers;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;
using OrchardCore.Recipes.Models;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.AuditTrail.Services
{
    public class AuditTrailManager : IAuditTrailManager
    {
        private readonly IClock _clock;
        private readonly IStringLocalizer T;
        private readonly YesSql.ISession _session;
        private readonly ISiteService _siteService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditTrailIdGenerator _auditTrailEventIdGenerator;
        private readonly IEnumerable<IAuditTrailEventHandler> _auditTrailEventHandlers;
        private readonly IEnumerable<IAuditTrailEventProvider> _auditTrailEventProviders;
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;

        public AuditTrailManager(
            IClock clock,
            YesSql.ISession session,
            ISiteService siteService,
            ILogger<AuditTrailManager> logger,
            IHttpContextAccessor httpContextAccessor,
            IStringLocalizer<AuditTrailManager> stringLocalizer,
            IAuditTrailIdGenerator auditTrailEventIdGenerator,
            IEnumerable<IAuditTrailEventHandler> auditTrailEventHandlers,
            IEnumerable<IAuditTrailEventProvider> auditTrailEventProviders,
            ShellSettings shellSettings)
        {
            _clock = clock;
            _session = session;
            _siteService = siteService;
            _httpContextAccessor = httpContextAccessor;
            _auditTrailEventHandlers = auditTrailEventHandlers;
            _auditTrailEventProviders = auditTrailEventProviders;
            _auditTrailEventIdGenerator = auditTrailEventIdGenerator;
            _shellSettings = shellSettings;
            _logger = logger;

            T = stringLocalizer;
        }

        public async Task RecordAuditTrailEventAsync<TAuditTrailEventProvider>(AuditTrailContext auditTrailContext)
            where TAuditTrailEventProvider : IAuditTrailEventProvider
        {
            if (_shellSettings.State == TenantState.Initializing && String.IsNullOrEmpty(auditTrailContext.UserName))
            {
                var feature = _httpContextAccessor.HttpContext.Features.Get<RecipeEnvironmentFeature>();
                if (feature != null && feature.Properties.TryGetValue(SetupConstants.AdminUsername, out var adminUsername))
                {
                    auditTrailContext.UserName = (string)adminUsername;
                }
            }

            var eventDescriptors = DescribeEvents(auditTrailContext.EventName, typeof(TAuditTrailEventProvider).FullName);
            foreach (var eventDescriptor in eventDescriptors)
            {
                if (!await IsEventEnabledAsync(eventDescriptor))
                {
                    continue;
                }

                var context = new AuditTrailCreateContext(
                    auditTrailContext.EventName,
                    auditTrailContext.UserName,
                    auditTrailContext.EventData,
                    auditTrailContext.EventFilterKey,
                    auditTrailContext.EventFilterData);

                await _auditTrailEventHandlers.InvokeAsync((handler, context) => handler.CreateAsync(context), context, _logger);

                var @event = new AuditTrailEvent
                {
                    AuditTrailEventId = _auditTrailEventIdGenerator.GenerateUniqueId(),
                    Category = eventDescriptor.CategoryDescriptor.Category,
                    EventName = context.EventName,
                    FullEventName = eventDescriptor.FullEventName,
                    UserName = context.UserName ?? "",
                    CreatedUtc = context.CreatedUtc ?? _clock.UtcNow,
                    Comment = context.Comment.NewlinesToHtml(),
                    EventFilterData = context.EventFilterData,
                    EventFilterKey = context.EventFilterKey,
                    ClientIpAddress = String.IsNullOrEmpty(context.ClientIpAddress)
                        ? await GetClientIpAddressAsync()
                        : context.ClientIpAddress
                };

                eventDescriptor.BuildAuditTrailEvent(@event, context.EventData);
                await _auditTrailEventHandlers.InvokeAsync((handler, context, @event) => handler.AlterAsync(context, @event), context, @event, _logger);
                _session.Save(@event, AuditTrailEvent.Collection);
            }
        }

        public async Task<AuditTrailEventSearchResults> GetAuditTrailEventsAsync(
            int page,
            int pageSize,
            Filters filters = null,
            AuditTrailOrderBy orderBy = AuditTrailOrderBy.DateDescending)
        {
            var query = _session.Query<AuditTrailEvent>(collection: AuditTrailEvent.Collection);

            if (filters != null)
            {
                var filterContext = new QueryFilterContext(query, filters);

                _auditTrailEventHandlers.Invoke((handler, context) => handler.FilterAsync(context), filterContext, _logger);

                query = filterContext.Query;
            }

            switch (orderBy)
            {
                case AuditTrailOrderBy.CategoryAscending:
                    query.With<AuditTrailEventIndex>().OrderBy(index => index.Category).ThenByDescending(index => index.CreatedUtc);
                    break;
                case AuditTrailOrderBy.EventAscending:
                    query.With<AuditTrailEventIndex>().OrderBy(index => index.EventName).ThenByDescending(index => index.CreatedUtc);
                    break;
                case AuditTrailOrderBy.DateDescending:
                    query.With<AuditTrailEventIndex>().OrderByDescending(index => index.Id);
                    break;
                default:
                    break;
            }

            var auditTrailEventsTotalCount = await query.CountAsync();

            if (pageSize > 0)
            {
                if (page > 1)
                {
                    query = query.Skip((page - 1) * pageSize);
                }

                query = query.Take(pageSize);
            }

            var auditTrailEvents = await query.ListAsync();

            return new AuditTrailEventSearchResults
            {
                AuditTrailEvents = auditTrailEvents,
                TotalCount = auditTrailEventsTotalCount
            };
        }

        public async Task<int> TrimAsync(TimeSpan retentionPeriod)
        {
            var dateThreshold = _clock.UtcNow.AddDays(1) - retentionPeriod;

            var auditTrailEvents = await _session.Query<AuditTrailEvent, AuditTrailEventIndex>(collection: AuditTrailEvent.Collection)
                .Where(index => index.CreatedUtc <= dateThreshold)
                .ListAsync();

            var deletedEvents = 0;
            foreach (var auditTrailEvent in auditTrailEvents)
            {
                _session.Delete(auditTrailEvent, collection: AuditTrailEvent.Collection);
                deletedEvents++;
            }

            return deletedEvents;
        }

        public Task<AuditTrailEvent> GetAuditTrailEventAsync(string auditTrailEventId) =>
            _session.Query<AuditTrailEvent, AuditTrailEventIndex>(collection: AuditTrailEvent.Collection)
                .Where(index => index.AuditTrailEventId == auditTrailEventId)
                .FirstOrDefaultAsync();

        public IEnumerable<AuditTrailCategoryDescriptor> DescribeCategories() => DescribeProviders().Describe();

        public DescribeContext DescribeProviders()
        {
            var describeContext = new DescribeContext();
            _auditTrailEventProviders.Invoke((provider, context) => provider.Describe(context), describeContext, _logger);
            return describeContext;
        }

        public AuditTrailEventDescriptor DescribeEvent(AuditTrailEvent auditTrailEvent) =>
            DescribeCategories()
                .SelectMany(categoryDescriptor => categoryDescriptor.Events
                    .Where(eventDescriptor => eventDescriptor.FullEventName == auditTrailEvent.FullEventName))
                .FirstOrDefault();

        private IEnumerable<AuditTrailEventDescriptor> DescribeEvents(string eventName, string providerName) =>
            DescribeCategories()
                .Where(categoryDescriptor => categoryDescriptor.Category == DescribeProviderCategory(providerName))
                .SelectMany(categoryDescriptor => categoryDescriptor.Events
                    .Where(eventDescriptor => eventDescriptor.EventName == eventName)
                .ToArray());

        private string DescribeProviderCategory(string providerName) =>
            DescribeProviders().Describe()
                .Where(category => category.ProviderName == providerName)
                .Select(categoryDescriptor => categoryDescriptor.Category)
                .FirstOrDefault();

        private async Task<string> GetClientIpAddressAsync()
        {
            var settings = await GetAuditTrailSettingsAsync();
            if (!settings.ClientIpAddressAllowed)
            {
                return null;
            }

            var address = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress;
            if (address != null)
            {
                if (IPAddress.IsLoopback(address))
                {
                    address = IPAddress.Loopback;
                }
            }

            return address?.ToString();
        }

        private async Task<AuditTrailSettings> GetAuditTrailSettingsAsync() =>
            (await _siteService.GetSiteSettingsAsync()).As<AuditTrailSettings>();

        private async Task<bool> IsEventEnabledAsync(AuditTrailEventDescriptor eventDescriptor)
        {
            if (eventDescriptor.IsMandatory)
            {
                return true;
            }

            var auditTrailSettings = await GetAuditTrailSettingsAsync();

            var auditTrailEventSetting = auditTrailSettings.EventSettings
                .FirstOrDefault(eventSetting => eventSetting.EventName == eventDescriptor.FullEventName);

            return auditTrailEventSetting != null ? auditTrailEventSetting.IsEnabled : eventDescriptor.IsEnabledByDefault;
        }
    }
}
