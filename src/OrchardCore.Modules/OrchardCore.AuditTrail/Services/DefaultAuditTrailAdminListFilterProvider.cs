using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.ViewModels;
using YesSql;
using YesSql.Filters.Query;
using YesSql.Services;

namespace OrchardCore.AuditTrail.Services
{
    public class DefaultAuditTrailAdminListFilterProvider : IAuditTrailAdminListFilterProvider
    {
        public void Build(QueryEngineBuilder<AuditTrailEvent> builder)
        {
            builder
                .WithNamedTerm("id", builder => builder
                    .OneCondition<AuditTrailEvent>((val, query) =>
                    {
                        if (!String.IsNullOrEmpty(val))
                        {
                            query.With<AuditTrailEventIndex>(x => x.CorrelationId == val);
                        }

                        return query;
                    })
                    .MapTo<AuditTrailIndexOptions>((val, model) =>
                    {
                        model.CorrelationId = val;
                    })
                    .MapFrom<AuditTrailIndexOptions>((model) =>
                    {
                        if (!String.IsNullOrEmpty(model.CorrelationId))
                        {
                            return (true, model.CorrelationId);
                        }
                        return (false, String.Empty);
                    })
                )
                .WithNamedTerm("category", builder => builder
                    .OneCondition<AuditTrailEvent>((val, query) =>
                    {
                        if (!String.IsNullOrEmpty(val))
                        {
                            query.With<AuditTrailEventIndex>(x => x.Category == val);
                        }

                        return query;
                    })
                    .MapTo<AuditTrailIndexOptions>((val, model) =>
                    {
                        model.Category = val;
                    })
                    .MapFrom<AuditTrailIndexOptions>((model) =>
                    {
                        if (!String.IsNullOrEmpty(model.Category))
                        {
                            return (true, model.Category);
                        }
                        return (false, String.Empty);
                    })
                )
                .WithNamedTerm("event", builder => builder
                    .OneCondition<AuditTrailEvent>((val, query) =>
                    {
                        if (!String.IsNullOrEmpty(val))
                        {
                            query.With<AuditTrailEventIndex>(x => x.Name == val);
                        }

                        return query;
                    })
                    .MapTo<AuditTrailIndexOptions>((val, model) =>
                    {
                        model.Event = val;
                    })
                    .MapFrom<AuditTrailIndexOptions>((model) =>
                    {
                        if (!String.IsNullOrEmpty(model.Event))
                        {
                            return (true, model.Event);
                        }
                        return (false, String.Empty);
                    })
                )
                .WithNamedTerm("date", builder => builder
                    .OneCondition<AuditTrailEvent>((val, query) =>
                    {
                        if (!String.IsNullOrEmpty(val))
                        {
                            DateTime? start = null;
                            DateTime? end = null;
                            var split = val.Split("..", 2);
                            if (split.Length == 1 && DateTime.TryParse(val, out var date))
                            {
                                start = date.Date;
                                end = date.Date.AddDays(1);
                            }
                            else if (split.Length == 2 && DateTime.TryParse(split[0], out var splitStart) && DateTime.TryParse(split[1], out var splitEnd))
                            {
                                start = splitStart;
                                // TODO if time component is zero add a day?
                                end = splitEnd.Date;
                            }

                            if (start != null && end != null)
                            {
                                query.With<AuditTrailEventIndex>(x => x.CreatedUtc >= start.GetValueOrDefault() && x.CreatedUtc < end.GetValueOrDefault());
                            }
                        }

                        return query;
                    })
                    .MapTo<AuditTrailIndexOptions>((val, model) =>
                    {
                        model.Date = val;
                    })
                    .MapFrom<AuditTrailIndexOptions>((model) =>
                    {
                        if (!String.IsNullOrEmpty(model.Date))
                        {
                            return (true, model.Date);
                        }
                        return (false, String.Empty);
                    })
                )
                .WithNamedTerm("sort", builder => builder
                    .OneCondition<AuditTrailEvent>((val, query) =>
                    {
                        if (Enum.TryParse<AuditTrailSort>(val, true, out var auditTrailSort))
                        {
                            switch (auditTrailSort)
                            {
                                case AuditTrailSort.Timestamp:
                                    query.With<AuditTrailEventIndex>().OrderByDescending(u => u.CreatedUtc);
                                    break;
                                case AuditTrailSort.Category:
                                    query.With<AuditTrailEventIndex>().OrderBy(index => index.Category).ThenByDescending(index => index.CreatedUtc);
                                    break;
                                case AuditTrailSort.Event:
                                    query.With<AuditTrailEventIndex>().OrderBy(index => index.Name).ThenByDescending(index => index.CreatedUtc);
                                    break;
                            };
                        }
                        else
                        {
                            query.With<AuditTrailEventIndex>().OrderByDescending(u => u.CreatedUtc);
                        }

                        return query;
                    })
                    .MapTo<AuditTrailIndexOptions>((val, model) =>
                    {
                        if (Enum.TryParse<AuditTrailSort>(val, true, out var sort))
                        {
                            model.Sort = sort;
                        }
                    })
                    .MapFrom<AuditTrailIndexOptions>((model) =>
                    {
                        if (model.Sort != AuditTrailSort.Timestamp)
                        {
                            return (true, model.Sort.ToString());
                        }

                        return (false, String.Empty);
                    })
                    .AlwaysRun()
                )
                .WithDefaultTerm("name", builder => builder
                    .ManyCondition<AuditTrailEvent>(
                        ((val, query, ctx) =>
                        {
                            // TODO normalized.
                            // var context = (AuditTrailQueryContext)ctx;
                            // var userManager = context.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                            query.With<AuditTrailEventIndex>(x => x.UserName.Contains(val));

                            return new ValueTask<IQuery<AuditTrailEvent>>(query);
                        }),
                        ((val, query, ctx) =>
                        {
                            // var context = (AuditTrailQueryContext)ctx;
                            // var userManager = context.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                            query.With<AuditTrailEventIndex>(x => x.UserName.IsNotIn<AuditTrailEventIndex>(s => s.UserName, w => w.UserName.Contains(val)));

                            return new ValueTask<IQuery<AuditTrailEvent>>(query);
                        })
                    )
                );
                // .WithNamedTerm("email", builder => builder
                //     .ManyCondition<AuditTrailEvent>(
                //         ((val, query, ctx) =>
                //         {
                //             // var context = (AuditTrailQueryContext)ctx;
                //             // var userManager = context.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                //             // query.With<UserIndex>(x => x.NormalizedEmail.Contains(val));

                //             return new ValueTask<IQuery<AuditTrailEvent>>(query);
                //         }),
                //         ((val, query, ctx) =>
                //         {
                //             // var context = (AuditTrailQueryContext)ctx;
                //             // var userManager = context.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                //             // query.With<UserIndex>(x => x.NormalizedEmail.IsNotIn<UserIndex>(s => s.NormalizedEmail, w => w.NormalizedEmail.Contains(val)));

                //             return new ValueTask<IQuery<AuditTrailEvent>>(query);
                //         })
                //     )
                // );

        }
    }

    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
