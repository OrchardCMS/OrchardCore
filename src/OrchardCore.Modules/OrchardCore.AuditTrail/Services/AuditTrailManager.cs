using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Providers;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;
using YesSql;
using IYesSqlSession = YesSql.ISession;

namespace OrchardCore.AuditTrail.Services
{
    public class AuditTrailManager : IAuditTrailManager
    {
        private readonly IClock _clock;
        private readonly IStringLocalizer T;
        private readonly IYesSqlSession _session;
        private readonly ISiteService _siteService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditTrailIdGenerator _auditTrailEventIdGenerator;
        private readonly IEnumerable<IAuditTrailEventHandler> _auditTrailEventHandlers;
        private readonly IEnumerable<IAuditTrailEventProvider> _auditTrailEventProviders;

        public ILogger Logger { get; set; }

        public AuditTrailManager(
            IClock clock,
            IYesSqlSession session,
            ISiteService siteService,
            ILogger<AuditTrailManager> logger,
            IHttpContextAccessor httpContextAccessor,
            IStringLocalizer<AuditTrailManager> stringLocalizer,
            IAuditTrailIdGenerator auditTrailEventIdGenerator,
            IEnumerable<IAuditTrailEventHandler> auditTrailEventHandlers,
            IEnumerable<IAuditTrailEventProvider> auditTrailEventProviders)
        {
            _clock = clock;
            _session = session;
            _siteService = siteService;
            _httpContextAccessor = httpContextAccessor;
            _auditTrailEventHandlers = auditTrailEventHandlers;
            _auditTrailEventProviders = auditTrailEventProviders;
            _auditTrailEventIdGenerator = auditTrailEventIdGenerator;

            Logger = logger;
            T = stringLocalizer;
        }

        public async Task AddAuditTrailEventAsync<TAuditTrailEventProvider>(AuditTrailContext auditTrailContext)
            where TAuditTrailEventProvider : IAuditTrailEventProvider
        {
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

                await _auditTrailEventHandlers.InvokeAsync((handler, context) => handler.CreateAsync(context), context, Logger);

                var @event = new AuditTrailEvent
                {
                    AuditTrailEventId = _auditTrailEventIdGenerator.GenerateUniqueId(),
                    Category = eventDescriptor.CategoryDescriptor.Category,
                    EventName = context.EventName,
                    FullEventName = eventDescriptor.FullEventName,
                    UserName = !String.IsNullOrEmpty(context.UserName)
                        ? context.UserName
                        : T["[empty]"],
                    CreatedUtc = context.CreatedUtc ?? _clock.UtcNow,
                    Comment = context.Comment.NewlinesToHtml(),
                    EventFilterData = context.EventFilterData,
                    EventFilterKey = context.EventFilterKey,
                    ClientIpAddress = String.IsNullOrEmpty(context.ClientIpAddress)
                        ? await GetClientAddressAsync()
                        : context.ClientIpAddress
                };

                eventDescriptor.BuildAuditTrailEvent(@event, context.EventData);

                await _auditTrailEventHandlers.InvokeAsync((handler, context, @event)
                    => handler.AlterAsync(context, @event), context, @event, Logger);

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

                _auditTrailEventHandlers.Invoke((handler, context) => handler.Filter(context), filterContext, Logger);

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

            var startIndex = (page - 1) * pageSize;
            query = query.Skip(startIndex);

            if (pageSize > 0)
            {
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
                .Where(index => index.CreatedUtc <= dateThreshold).ListAsync();

            var deletedEvents = 0;

            // Related Orchard Core issue to be able to delete items without a foreach:
            // https://github.com/OrchardCMS/OrchardCore/issues/5821
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
            _auditTrailEventProviders.Invoke((provider, context) => provider.Describe(context), describeContext, Logger);
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
                    .Where(eventDescriptor => eventDescriptor.EventName == eventName));

        private string DescribeProviderCategory(string providerName) =>
            DescribeProviders().Describe()
                .Where(category => category.ProviderName == providerName)
                .Select(categoryDescriptor => categoryDescriptor.Category)
                .FirstOrDefault();

        private async Task<string> GetClientAddressAsync()
        {
            var settings = await GetAuditTrailSettingsAsync();

            if (!settings.EnableClientIpAddressLogging)
            {
                return null;
            }

            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
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
