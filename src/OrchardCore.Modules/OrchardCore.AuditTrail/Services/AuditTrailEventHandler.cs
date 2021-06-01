using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.AuditTrail.Services
{
    public class AuditTrailEventHandler : AuditTrailEventHandlerBase
    {
        public AuditTrailEventHandler(IStringLocalizer<AuditTrailEventHandler> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public override Task FilterAsync(QueryFilterContext context)
        {
            var userName = context.Filters.Get("username")?.Trim();
            var category = context.Filters.Get("category");
            var from = GetDateFromFilter(context.Filters, "From", "from");
            var to = GetDateFromFilter(context.Filters, "To", "to");

            // if (!String.IsNullOrWhiteSpace(userName))
            // {
            //     context.Query.With<AuditTrailEventIndex>(index => index.UserName == userName);
            // }

            if (!String.IsNullOrWhiteSpace(category))
            {
                context.Query.With<AuditTrailEventIndex>(index => index.Category == category);
            }

            if (from != null)
            {
                context.Query.With<AuditTrailEventIndex>(index => index.CreatedUtc >= from);
            }

            if (to != null)
            {
                context.Query.With<AuditTrailEventIndex>(index => index.CreatedUtc <= to.Value.AddDays(1));
            }

            return Task.CompletedTask;
        }

        private DateTime? GetDateFromFilter(Filters filters, string fieldName, string prefix)
        {
            var dateString = filters.Get(prefix + "Date");

            if (String.IsNullOrEmpty(dateString)) return null;

            try
            {
                return DateTime.Parse(dateString).ToUniversalTime();
            }
            catch (FormatException ex)
            {
                filters.UpdateModel.ModelState.AddModelError(prefix, S["Error parsing '{0}' date string '{1}': {2}", fieldName, dateString, ex.Message]);
                return null;
            }
        }
    }
}
