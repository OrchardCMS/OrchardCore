using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.AuditTrail.Services
{
    public class CommonAuditTrailEventHandler : AuditTrailEventHandlerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public CommonAuditTrailEventHandler(
            IServiceProvider serviceProvider,
            IStringLocalizer<CommonAuditTrailEventHandler> stringLocalizer)
        {
            _serviceProvider = serviceProvider;

            T = stringLocalizer;
        }

        public override void Filter(QueryFilterContext context)
        {
            var userName = context.Filters.Get("username")?.Trim();
            var category = context.Filters.Get("category");
            var from = GetDateFromFilter(context.Filters, "From", "from");
            var to = GetDateFromFilter(context.Filters, "To", "to");

            if (!string.IsNullOrWhiteSpace(userName))
            {
                context.Query.With<AuditTrailEventIndex>(eventIndex => eventIndex.UserName == userName);
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                context.Query.With<AuditTrailEventIndex>(eventIndex => eventIndex.Category == category);
            }

            if (from != null)
            {
                context.Query.With<AuditTrailEventIndex>(eventIndex => eventIndex.CreatedUtc >= from);
            }

            if (to != null)
            {
                context.Query.With<AuditTrailEventIndex>(eventIndex => eventIndex.CreatedUtc <= to.Value.AddDays(1));
            }
        }

        public override async Task DisplayFilterAsync(DisplayFilterContext context)
        {
            var auditTrailManager = _serviceProvider.GetService<IAuditTrailManager>();

            var userName = context.Filters.Get("username");
            var fromDate = context.Filters.Get("fromDate");
            var toDate = context.Filters.Get("toDate");
            var category = context.Filters.Get("category");

            var userNameFilterDisplay = await context.ShapeFactory.New.AuditTrailFilter__Common__User(UserName: userName);
            var dateFromFilterDisplay = await context.ShapeFactory.New.AuditTrailFilter__Common__Date__From(Date: fromDate);
            var dateToFilterDisplay = await context.ShapeFactory.New.AuditTrailFilter__Common__Date__To(Date: toDate);
            var categoryFilterDisplay = await context.ShapeFactory.New.AuditTrailFilter__Common__Category(
                Categories: auditTrailManager.DescribeCategories().ToArray(),
                Category: category);

            context.FilterDisplay.Add(dateFromFilterDisplay);
            context.FilterDisplay.Add(dateToFilterDisplay);
            context.FilterDisplay.Add(categoryFilterDisplay);
            context.FilterDisplay.Add(userNameFilterDisplay);
        }

        private DateTime? GetDateFromFilter(Filters filters, string fieldName, string prefix)
        {
            var dateString = filters.Get(prefix + "Date");

            if (string.IsNullOrEmpty(dateString)) return null;

            try
            {
                return DateTime.Parse(dateString).ToUniversalTime();
            }
            catch (FormatException ex)
            {
                filters.UpdateModel.ModelState.AddModelError(prefix, T["Error parsing '{0}' date string '{1}': {2}", fieldName, dateString, ex.Message]);
                return null;
            }
        }
    }
}
