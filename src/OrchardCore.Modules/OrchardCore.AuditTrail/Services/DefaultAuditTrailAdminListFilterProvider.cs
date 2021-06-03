using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.ViewModels;
using OrchardCore.Modules;
using YesSql;
using YesSql.Filters.Query;
using YesSql.Services;
using Parlot.Fluent;

namespace OrchardCore.AuditTrail.Services
{
    public class DefaultAuditTrailAdminListFilterProvider : IAuditTrailAdminListFilterProvider
    {

        // private static List<Func<string, DateTimeOffset, (bool IncludesTime, DateTime? Value)>> DateParsers = new List<Func<string, DateTimeOffset, (bool IncludesTime, DateTime? Value)>>()
        // {
        //     (val, localNow) =>
        //     {
        //         // Try with a round trip ISO 8601.
        //         if (DateTimeOffset.TryParseExact(val, "o", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeOffset))
        //         {
        //             return (true, dateTimeOffset.UtcDateTime);
        //         }

        //         return (true, null);
        //     },
        //     (val, localNow) =>
        //     {
        //         // Try with a date only from the ISO 8601 format.
        //         if (DateTime.TryParseExact(val, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        //         {
        //             var dateTimeOffset = new DateTimeOffset(parsedDate.Year, parsedDate.Month, parsedDate.Day, localNow.Hour, localNow.Minute, localNow.Second, localNow.Offset);
        //             return (false, dateTimeOffset.UtcDateTime);
        //         }

        //         return (false, null);
        //     }
        // };

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
                    .OneCondition<AuditTrailEvent>((val, query, ctx) =>
                    {
                        if (!string.IsNullOrEmpty(val) && DateTimeParser.Parser.TryParse(val, out var expression))
                        {
                            var context = (AuditTrailQueryContext)ctx;
                            var clock = context.ServiceProvider.GetRequiredService<IClock>();
                            var utcNow = clock.UtcNow;

                            var param = Expression.Parameter(typeof(AuditTrailEventIndex));
                            var field = Expression.Property(param, nameof(AuditTrailEventIndex.CreatedUtc));
                            var expressionContext = new BuildExpressionContext(utcNow, param, field, typeof(Func<AuditTrailEventIndex, bool>));

                            var builtLambda = expression.BuildExpression(expressionContext);

                            var casted = (Expression<Func<AuditTrailEventIndex, bool>>)builtLambda;
                            query.With<AuditTrailEventIndex>(casted);
                        }

                        return new ValueTask<IQuery<AuditTrailEvent>>(query);
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
                .WithDefaultTerm("username", builder => builder
                    .ManyCondition<AuditTrailEvent>(
                        (val, query, ctx) =>
                        {
                            var context = (AuditTrailQueryContext)ctx;
                            var lookupNormalizer = context.ServiceProvider.GetRequiredService<ILookupNormalizer>();
                            var normalizedUserName = lookupNormalizer.NormalizeName(val);
                            query.With<AuditTrailEventIndex>(x => x.NormalizedUserName.Contains(normalizedUserName));

                            return new ValueTask<IQuery<AuditTrailEvent>>(query);
                        },
                        (val, query, ctx) =>
                        {
                            var context = (AuditTrailQueryContext)ctx;
                            var lookupNormalizer = context.ServiceProvider.GetRequiredService<ILookupNormalizer>();
                            var normalizedUserName = lookupNormalizer.NormalizeName(val);
                            query.With<AuditTrailEventIndex>(x => x.NormalizedUserName.IsNotIn<AuditTrailEventIndex>(s => s.NormalizedUserName, w => w.NormalizedUserName.Contains(normalizedUserName)));

                            return new ValueTask<IQuery<AuditTrailEvent>>(query);
                        }
                    )
                )
                .WithNamedTerm("userid", builder => builder
                    .ManyCondition<AuditTrailEvent>(
                        (val, query) =>
                        {
                            query.With<AuditTrailEventIndex>(x => x.UserId.Contains(val));

                            return query;
                        },
                        (val, query) =>
                        {
                            query.With<AuditTrailEventIndex>(x => x.UserId.IsNotIn<AuditTrailEventIndex>(s => s.UserId, w => w.UserId.Contains(val)));

                            return query;
                        }
                    )
                );
        }
    }
}
