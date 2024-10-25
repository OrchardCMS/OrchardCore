using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.AuditTrail.ViewModels;
using OrchardCore.Modules;
using Parlot;
using YesSql.Filters.Query;
using YesSql.Services;

namespace OrchardCore.AuditTrail.Services;

public class DefaultAuditTrailAdminListFilterProvider : IAuditTrailAdminListFilterProvider
{
    private readonly IOptions<AuditTrailAdminListOptions> _options;

    public DefaultAuditTrailAdminListFilterProvider(IOptions<AuditTrailAdminListOptions> options)
    {
        _options = options;
    }

    public void Build(QueryEngineBuilder<AuditTrailEvent> builder)
    {
        builder
            .WithNamedTerm("id", builder => builder
                .OneCondition((val, query) =>
                {
                    if (!string.IsNullOrEmpty(val))
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
                    if (!string.IsNullOrEmpty(model.CorrelationId))
                    {
                        return (true, model.CorrelationId);
                    }
                    return (false, string.Empty);
                })
            )
            .WithNamedTerm("category", builder => builder
                .OneCondition((val, query) =>
                {
                    if (!string.IsNullOrEmpty(val))
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
                    if (!string.IsNullOrEmpty(model.Category))
                    {
                        return (true, model.Category);
                    }
                    return (false, string.Empty);
                })
            )
            .WithNamedTerm("event", builder => builder
                .OneCondition((val, query) =>
                {
                    if (!string.IsNullOrEmpty(val))
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
                    if (!string.IsNullOrEmpty(model.Event))
                    {
                        return (true, model.Event);
                    }
                    return (false, string.Empty);
                })
            )
            .WithNamedTerm("date", builder => builder
                .OneCondition(async (val, query, ctx) =>
                {
                    if (string.IsNullOrEmpty(val))
                    {
                        return query;
                    }

                    var context = (AuditTrailQueryContext)ctx;
                    var clock = context.ServiceProvider.GetRequiredService<IClock>();
                    var localClock = context.ServiceProvider.GetRequiredService<ILocalClock>();
                    var userTimeZone = await localClock.GetLocalTimeZoneAsync();
                    var parseContext = new DateTimeParseContext(CultureInfo.CurrentUICulture, clock, userTimeZone, new Scanner(val));

                    if (DateTimeParser.Parser.TryParse(parseContext, out var expression, out var parseError))
                    {
                        var utcNow = clock.UtcNow;

                        var param = Expression.Parameter(typeof(AuditTrailEventIndex));
                        var field = Expression.Property(param, nameof(AuditTrailEventIndex.CreatedUtc));
                        var expressionContext = new BuildExpressionContext(utcNow, param, field, typeof(Func<AuditTrailEventIndex, bool>));

                        query.With<AuditTrailEventIndex>((Expression<Func<AuditTrailEventIndex, bool>>)expression.BuildExpression(expressionContext));
                    }

                    return query;
                })
                .MapTo<AuditTrailIndexOptions>((val, model) =>
                {
                    model.Date = val;
                })
                .MapFrom<AuditTrailIndexOptions>((model) =>
                {
                    if (!string.IsNullOrEmpty(model.Date))
                    {
                        return (true, model.Date);
                    }
                    return (false, string.Empty);
                })
            )
            .WithNamedTerm("sort", builder => builder
                .OneCondition((val, query, ctx) =>
                {
                    var context = (AuditTrailQueryContext)ctx;
                    var options = context.ServiceProvider.GetRequiredService<IOptions<AuditTrailAdminListOptions>>().Value;

                    if (options.SortOptions.TryGetValue(val, out var sortOption))
                    {
                        return sortOption.Query(val, query, ctx);
                    }

                    return options.DefaultSortOption.Query(val, query, ctx);
                })
                .MapTo<AuditTrailIndexOptions>((val, model) =>
                {
                    // TODO add a context property to the mapping func.
                    if (!string.IsNullOrEmpty(val) && _options.Value.SortOptions.TryGetValue(val, out var sortOption))
                    {
                        model.Sort = sortOption.Value;
                    }
                })
                .MapFrom<AuditTrailIndexOptions>((model) =>
                {
                    // TODO add a context property to the mapping func.
                    if (model.Sort != _options.Value.DefaultSortOption.Value)
                    {
                        return (true, model.Sort);
                    }

                    return (false, string.Empty);
                })
                .AlwaysRun()
            )
            .WithDefaultTerm("username", builder => builder
                .ManyCondition(
                    (val, query, ctx) =>
                    {
                        var context = (AuditTrailQueryContext)ctx;
                        var lookupNormalizer = context.ServiceProvider.GetRequiredService<ILookupNormalizer>();
                        var normalizedUserName = lookupNormalizer.NormalizeName(val);
                        query.With<AuditTrailEventIndex>(x => x.NormalizedUserName.Contains(normalizedUserName));

                        return ValueTask.FromResult(query);
                    },
                    (val, query, ctx) =>
                    {
                        var context = (AuditTrailQueryContext)ctx;
                        var lookupNormalizer = context.ServiceProvider.GetRequiredService<ILookupNormalizer>();
                        var normalizedUserName = lookupNormalizer.NormalizeName(val);
                        query.With<AuditTrailEventIndex>(x => x.NormalizedUserName.NotContains(normalizedUserName));

                        return ValueTask.FromResult(query);
                    }
                )
            )
            .WithNamedTerm("userid", builder => builder
                .ManyCondition(
                    (val, query) =>
                    {
                        query.With<AuditTrailEventIndex>(x => x.UserId.Contains(val));

                        return query;
                    },
                    (val, query) =>
                    {
                        query.With<AuditTrailEventIndex>(x => x.UserId.NotContains(val));

                        return query;
                    }
                )
            );
    }
}
