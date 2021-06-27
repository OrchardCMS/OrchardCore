using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.AuditTrail.ViewModels;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.AuditTrail.Services
{
    public class DefaultAuditTrailAdminListQueryService : IAuditTrailAdminListQueryService
    {
        private const string DateFormat = "yyyy-MM-dd";
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly ILocalClock _localClock;
        private readonly AuditTrailAdminListOptions _adminListOptions;
        private readonly ISession _session;
        private readonly IServiceProvider _serviceProvider;
        private readonly IStringLocalizer S;
        private readonly ILogger _logger;

        public DefaultAuditTrailAdminListQueryService(
            IAuditTrailManager auditTrailManager,
            ILocalClock localClock,
            IOptions<AuditTrailAdminListOptions> adminListOptions,
            ISession session,
            IServiceProvider serviceProvider,
            IStringLocalizer<DefaultAuditTrailAdminListQueryService> stringLocalizer,
            ILogger<DefaultAuditTrailAdminListQueryService> logger)
        {
            _auditTrailManager = auditTrailManager;
            _localClock = localClock;
            _adminListOptions = adminListOptions.Value;
            _session = session;
            _serviceProvider = serviceProvider;
            S = stringLocalizer;
            _logger = logger;
        }

        public async Task<AuditTrailEventQueryResult> QueryAsync(int page, int pageSize, AuditTrailIndexOptions options)
        {
            // Because admin filters can add a different index to the query this must be added as a Query<AuditTrailEvent>()
            var query = _session.Query<AuditTrailEvent>(collection: AuditTrailEvent.Collection);

            query = await options.FilterResult.ExecuteAsync(new AuditTrailQueryContext(_serviceProvider, query));

            var totalCount = await query.CountAsync();

            if (pageSize > 0)
            {
                if (page > 1)
                {
                    query = query.Skip((page - 1) * pageSize);
                }

                query = query.Take(pageSize);
            }

            var events = await query.ListAsync();

            var result = new AuditTrailEventQueryResult
            {
                Events = events,
                TotalCount = totalCount
            };

            options.AuditTrailSorts = _adminListOptions.SortOptions.Values.Where(x => x.SelectListItem != null).Select(opt => opt.SelectListItem(_serviceProvider, opt, options)).ToList();

            var categories = _auditTrailManager.DescribeCategories();

            options.Categories = categories
                .Select(category => new SelectListItem(category.LocalizedName(_serviceProvider), category.Name, category.Name == options.Category))
                .ToList();

            options.Categories.Insert(0, new SelectListItem(S["All categories"], String.Empty, String.IsNullOrEmpty(options.Category)));

            if (options.CorrelationIdFromRoute)
            {
                var firstEvent = result.Events.FirstOrDefault();
                if (firstEvent != null)
                {
                    var currentCategory = categories.FirstOrDefault(x => x.Name == firstEvent.Category);
                    if (currentCategory != null)
                    {
                        options.Events = currentCategory.Events.Values.Select(category =>
                            new SelectListItem(category.LocalizedName(_serviceProvider), category.Name, category.Name == options.Category)).ToList();
                    }
                }
            }

            var localNow = await _localClock.LocalNowAsync;
            var startOfWeek = CultureInfo.CurrentUICulture.DateTimeFormat.FirstDayOfWeek;
            options.AuditTrailDates = new List<SelectListItem>()
            {
                new SelectListItem(S["Any date"], String.Empty, options.Date == String.Empty),
            };

            var dateTimeValue = ">@now-1";
            options.AuditTrailDates.Add(new SelectListItem(S["Last 24 hours"], dateTimeValue, options.Date == dateTimeValue));

            dateTimeValue = "@now-2..@now-1";
            options.AuditTrailDates.Add(new SelectListItem(S["Previous 48 hours"], dateTimeValue, options.Date == dateTimeValue));

            dateTimeValue = $">{localNow.StartOfWeek(CultureInfo.CurrentUICulture.DateTimeFormat.FirstDayOfWeek).LocalDateTime.ToString(DateFormat)}";
            options.AuditTrailDates.Add(new SelectListItem(S["This week"], dateTimeValue, options.Date == dateTimeValue));

            var start = localNow.StartOfWeek(CultureInfo.CurrentUICulture.DateTimeFormat.FirstDayOfWeek).AddDays(-7).LocalDateTime;
            var end = start.AddDays(7);
            dateTimeValue = $"{start.ToString(DateFormat)}..{end.ToString(DateFormat)}";
            options.AuditTrailDates.Add(new SelectListItem(S["Last week"], dateTimeValue, options.Date == dateTimeValue));

            start = new DateTime(localNow.LocalDateTime.Year, localNow.LocalDateTime.Month, 1);
            dateTimeValue = $">{start.ToString(DateFormat)}";
            options.AuditTrailDates.Add(new SelectListItem(S["This month"], dateTimeValue, options.Date == dateTimeValue));

            start = new DateTime(localNow.LocalDateTime.Year, localNow.LocalDateTime.Month, 1).AddMonths(-1);
            end = start.AddMonths(1);
            dateTimeValue = $"{start.ToString(DateFormat)}..{end.ToString(DateFormat)}";
            options.AuditTrailDates.Add(new SelectListItem(S["Last month"], dateTimeValue, options.Date == dateTimeValue));
            options.AuditTrailDates.Add(new SelectListItem(S["Custom date range"], dateTimeValue, options.Date == dateTimeValue));

            dateTimeValue = $">{localNow.AddHours(-1).ToString("o")}";
            options.AuditTrailDates.Add(new SelectListItem(S["Last hour"], dateTimeValue, options.Date == dateTimeValue));

            dateTimeValue = $"{localNow.AddHours(-2).ToString("o")}..{localNow.AddHours(-1).ToString("o")}";
            options.AuditTrailDates.Add(new SelectListItem(S["Previous hour"], dateTimeValue, options.Date == dateTimeValue));
            options.AuditTrailDates.Add(new SelectListItem(S["Custom time range"], dateTimeValue, options.Date == dateTimeValue));

            return result;
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTimeOffset StartOfWeek(this DateTimeOffset dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
