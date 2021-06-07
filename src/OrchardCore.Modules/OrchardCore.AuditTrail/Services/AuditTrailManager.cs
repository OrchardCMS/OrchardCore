using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrchardCore.Abstractions.Setup;
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
        private readonly YesSql.ISession _session;
        private readonly ISiteService _siteService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILookupNormalizer _keyNormalizer;
        private readonly IAuditTrailIdGenerator _auditTrailIdGenerator;
        private readonly IEnumerable<IAuditTrailEventHandler> _auditTrailEventHandlers;
        private readonly IEnumerable<IAuditTrailEventProvider> _auditTrailEventProviders;
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;

        public AuditTrailManager(
            IClock clock,
            YesSql.ISession session,
            ISiteService siteService,
            IHttpContextAccessor httpContextAccessor,
            ILookupNormalizer keyNormalizer,
            IEnumerable<IAuditTrailEventHandler> auditTrailEventHandlers,
            IEnumerable<IAuditTrailEventProvider> auditTrailEventProviders,
            IAuditTrailIdGenerator auditTrailIdGenerator,
            ShellSettings shellSettings,
            ILogger<AuditTrailManager> logger)
        {
            _clock = clock;
            _session = session;
            _siteService = siteService;
            _httpContextAccessor = httpContextAccessor;
            _keyNormalizer = keyNormalizer;
            _auditTrailEventHandlers = auditTrailEventHandlers;
            _auditTrailEventProviders = auditTrailEventProviders;
            _auditTrailIdGenerator = auditTrailIdGenerator;
            _shellSettings = shellSettings;
            _logger = logger;
        }

        public async Task RecordEventAsync(AuditTrailContext context)
        {
            if (_shellSettings.State == TenantState.Initializing && String.IsNullOrEmpty(context.UserName))
            {
                var feature = _httpContextAccessor.HttpContext.Features.Get<RecipeEnvironmentFeature>();
                if (feature != null && feature.Properties.TryGetValue(SetupConstants.AdminUsername, out var adminUsername))
                {
                    context.UserName = (string)adminUsername;
                }
            }

            var descriptor = DescribeEvent(context.Name, context.Category);
            if (descriptor == null || !await IsEventEnabledAsync(descriptor))
            {
                return;
            }

            var createContext = new AuditTrailCreateContext(
                context.Name,
                context.Category,
                context.CorrelationId,
                context.UserId,
                context.UserName,
                context.Data);

            await _auditTrailEventHandlers.InvokeAsync((handler, context) => handler.CreateAsync(context), createContext, _logger);

            var @event = new AuditTrailEvent
            {
                EventId = _auditTrailIdGenerator.GenerateUniqueId(),
                Category = createContext.Category,
                Name = createContext.Name,
                CorrelationId = createContext.CorrelationId,
                UserId = createContext.UserId,
                UserName = createContext.UserName ?? "",
                NormalizedUserName = String.IsNullOrEmpty(createContext.UserName) ? "" : _keyNormalizer.NormalizeName(createContext.UserName),
                ClientIpAddress = String.IsNullOrEmpty(createContext.ClientIpAddress)
                    ? await GetClientIpAddressAsync()
                    : createContext.ClientIpAddress,
                CreatedUtc = createContext.CreatedUtc ?? _clock.UtcNow,
                Comment = createContext.Comment
            };

            descriptor.BuildEvent(@event, createContext.Data);
            await _auditTrailEventHandlers.InvokeAsync((handler, context, @event) => handler.AlterAsync(context, @event), createContext, @event, _logger);

            _session.Save(@event, AuditTrailEvent.Collection);
        }

        public Task<AuditTrailEvent> GetEventAsync(string eventId) =>
            _session.Query<AuditTrailEvent, AuditTrailEventIndex>(collection: AuditTrailEvent.Collection)
                .Where(index => index.EventId == eventId)
                .FirstOrDefaultAsync();

        public async Task<int> TrimEventsAsync(TimeSpan retentionPeriod)
        {
            var dateThreshold = _clock.UtcNow.AddDays(1) - retentionPeriod;

            var events = await _session.Query<AuditTrailEvent, AuditTrailEventIndex>(collection: AuditTrailEvent.Collection)
                .Where(index => index.CreatedUtc <= dateThreshold)
                .ListAsync();

            var deletedEvents = 0;
            foreach (var @event in events)
            {
                _session.Delete(@event, collection: AuditTrailEvent.Collection);
                deletedEvents++;
            }

            return deletedEvents;
        }

        public AuditTrailEventDescriptor DescribeEvent(AuditTrailEvent @event) =>
            DescribeEvent(@event.Name, @event.Category)
            ?? AuditTrailEventDescriptor.Default(@event);

        public IEnumerable<AuditTrailCategoryDescriptor> DescribeCategories() =>
            DescribeProviders().Describe();

        public AuditTrailCategoryDescriptor DescribeCategory(string name) =>
            DescribeProviders(name).Describe()
                .FirstOrDefault(category => category.Name == name)
                ?? AuditTrailCategoryDescriptor.Default(name);

        private AuditTrailEventDescriptor DescribeEvent(string name, string categoryName) =>
            DescribeProviders(categoryName).Describe()
                .FirstOrDefault(category => category.Name == categoryName)?.Events
                .FirstOrDefault(@event => @event.Name == name);

        private DescribeContext DescribeProviders(string category = null)
        {
            var context = new DescribeContext() { Category = category };
            _auditTrailEventProviders.Invoke((provider, context) => provider.Describe(context), context, _logger);
            return context;
        }

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

        private async Task<bool> IsEventEnabledAsync(AuditTrailEventDescriptor descriptor)
        {
            if (descriptor.IsMandatory)
            {
                return true;
            }

            var settings = await GetAuditTrailSettingsAsync();

            var eventSettings = settings.Categories
                .FirstOrDefault(category => category.Name == descriptor.Category)?.Events
                .FirstOrDefault(settings => settings.Name == descriptor.Name);

            return eventSettings?.IsEnabled ?? descriptor.IsEnabledByDefault;
        }
    }
}
