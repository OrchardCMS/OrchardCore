using System;
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
                .WithNamedTerm("status", builder => builder
                    .OneCondition<AuditTrailEvent>((val, query) =>
                    {
                        // if (Enum.TryParse<UsersFilter>(val, true, out var usersStatus))
                        // {
                        //     switch (usersStatus)
                        //     {
                        //         case UsersFilter.Enabled:
                        //             query.With<UserIndex>(u => u.IsEnabled);
                        //             break;
                        //         case UsersFilter.Disabled:
                        //             query.With<UserIndex>(u => !u.IsEnabled);
                        //             break;
                        //     }
                        // }

                        return query;
                    })
                    .MapTo<AuditTrailIndexOptions>((val, model) =>
                    {
                        // if (Enum.TryParse<UsersFilter>(val, true, out var usersFilter))
                        // {
                        //     model.Filter = usersFilter;
                        // }
                    })
                    .MapFrom<AuditTrailIndexOptions>((model) =>
                    {
                        // switch (model.Filter)
                        // {
                        //     case UsersFilter.Enabled:
                        //         return (true, model.Filter.ToString());
                        //     case UsersFilter.Disabled:
                        //         return (true, model.Filter.ToString());
                        // }

                        return (false, String.Empty);
                    })
                )
                .WithNamedTerm("sort", builder => builder
                    .OneCondition<AuditTrailEvent>((val, query) =>
                    {
                        // if (Enum.TryParse<UsersOrder>(val, true, out var usersOrder))
                        // {
                        //     switch (usersOrder)
                        //     {
                        //         case UsersOrder.Name:
                        //             query.With<UserIndex>().OrderBy(u => u.NormalizedUserName);
                        //             break;
                        //         // Name is provided as a default sort.
                        //         case UsersOrder.Email:
                        //             query.With<UserIndex>().OrderBy(u => u.NormalizedEmail);
                        //             break;
                        //     };
                        // }
                        // else
                        // {
                        //     query.With<UserIndex>().OrderBy(u => u.NormalizedUserName);
                        // }

                        return query;
                    })
                    .MapTo<AuditTrailIndexOptions>((val, model) =>
                    {
                        // if (Enum.TryParse<UsersOrder>(val, true, out var order))
                        // {
                        //     model.Order = order;
                        // }
                    })
                    .MapFrom<AuditTrailIndexOptions>((model) =>
                    {
                        // if (model.Order != UsersOrder.Name)
                        // {
                        //     return (true, model.Order.ToString());
                        // }

                        return (false, String.Empty);
                    })
                    .AlwaysRun()
                )
                .WithDefaultTerm("name", builder => builder
                    .ManyCondition<AuditTrailEvent>(
                        ((val, query, ctx) =>
                        {
                            // var context = (AuditTrailQueryContext)ctx;
                            // var userManager = context.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                            // query.With<UserIndex>(x => x.NormalizedUserName.Contains(val));

                            return new ValueTask<IQuery<AuditTrailEvent>>(query);
                        }),
                        ((val, query, ctx) =>
                        {
                            // var context = (AuditTrailQueryContext)ctx;
                            // var userManager = context.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                            // query.With<UserIndex>(x => x.NormalizedUserName.IsNotIn<UserIndex>(s => s.NormalizedUserName, w => w.NormalizedUserName.Contains(val)));

                            return new ValueTask<IQuery<AuditTrailEvent>>(query);
                        })
                    )
                )
                .WithNamedTerm("email", builder => builder
                    .ManyCondition<AuditTrailEvent>(
                        ((val, query, ctx) =>
                        {
                            // var context = (AuditTrailQueryContext)ctx;
                            // var userManager = context.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                            // query.With<UserIndex>(x => x.NormalizedEmail.Contains(val));

                            return new ValueTask<IQuery<AuditTrailEvent>>(query);
                        }),
                        ((val, query, ctx) =>
                        {
                            // var context = (AuditTrailQueryContext)ctx;
                            // var userManager = context.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                            // query.With<UserIndex>(x => x.NormalizedEmail.IsNotIn<UserIndex>(s => s.NormalizedEmail, w => w.NormalizedEmail.Contains(val)));

                            return new ValueTask<IQuery<AuditTrailEvent>>(query);
                        })
                    )
                );

        }
    }
}
