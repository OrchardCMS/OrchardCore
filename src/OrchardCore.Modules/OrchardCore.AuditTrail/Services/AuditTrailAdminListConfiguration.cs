using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.AuditTrail.Services
{
    public class AuditTrailAdminListConfiguration : IConfigureOptions<AuditTrailAdminListOptions>
    {
        public void Configure(AuditTrailAdminListOptions options)
        {
            options.ForSort("time-desc")
                .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderByDescending(i => i.CreatedUtc))
                .WithSelectListItem((sp, opt, model) =>
                {
                    var S = sp.GetRequiredService<IStringLocalizer<AuditTrailAdminListConfiguration>>();
                    return new SelectListItem(S["Newest"], opt.Value, model.Sort == String.Empty);
                })
                .AsDefault();

            options.ForSort("time-asc")
                .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderBy(i => i.CreatedUtc))
                .WithSelectListItem((sp, opt, model) =>
                {
                    var S = sp.GetRequiredService<IStringLocalizer<AuditTrailAdminListConfiguration>>();
                    return new SelectListItem(S["Oldest"], opt.Value, model.Sort == opt.Value);
                });

            options.ForSort("category-asc-time-desc")
                .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderBy(i => i.Category).ThenByDescending(i => i.CreatedUtc))
                .WithSelectListItem((sp, opt, model) =>
                {
                    var S = sp.GetRequiredService<IStringLocalizer<AuditTrailAdminListConfiguration>>();
                    return new SelectListItem(S["Category"], opt.Value,  model.Sort == opt.Value);
                });

            options.ForSort("category-asc-time-asc")
                .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderBy(i => i.Category).ThenBy(i => i.CreatedUtc));

            options.ForSort("category-desc-time-desc")
                .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderByDescending(i => i.Category).ThenByDescending(i => i.CreatedUtc));

            options.ForSort("category-desc-time-asc")
                .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderByDescending(i => i.Category).ThenBy(i => i.CreatedUtc));

            options.ForSort("event-asc-time-desc")
                .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderBy(i => i.Name).ThenByDescending(i => i.CreatedUtc))
                .WithSelectListItem((sp, opt, model) =>
                {
                    var S = sp.GetRequiredService<IStringLocalizer<AuditTrailAdminListConfiguration>>();
                    return new SelectListItem(S["Event"], opt.Value, model.Sort == opt.Value);
                });

            options.ForSort("event-asc-time-asc")
                .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderBy(i => i.Name).ThenBy(i => i.CreatedUtc));

            options.ForSort("event-desc-time-desc")
                .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderByDescending(i => i.Name).ThenByDescending(i => i.CreatedUtc));

            options.ForSort("event-desc-time-asc")
                .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderByDescending(i => i.Name).ThenBy(i => i.CreatedUtc));

            options.ForSort("user-asc-time-asc")
                .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderBy(i => i.NormalizedUserName).ThenBy(i => i.CreatedUtc))
                .WithSelectListItem((sp, opt, model) =>
                {
                    var S = sp.GetRequiredService<IStringLocalizer<AuditTrailAdminListConfiguration>>();
                    return new SelectListItem(S["User"], opt.Value, model.Sort == opt.Value);
                });

            options.ForSort("user-desc-time-desc")
                .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderByDescending(i => i.NormalizedUserName).ThenByDescending(i => i.CreatedUtc));

            options.ForSort("user-desc-time-asc")
                .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderByDescending(i => i.NormalizedUserName).ThenBy(i => i.CreatedUtc));

            options.ForSort("user-desc-time-desc")
                .WithQuery((val, query) => query.With<AuditTrailEventIndex>().OrderByDescending(i => i.NormalizedUserName).ThenByDescending(i => i.CreatedUtc));
        }
    }
}
