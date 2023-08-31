using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;
using YesSql;
using YesSql.Filters.Query;
using YesSql.Services;

namespace OrchardCore.Users.Services
{
    public class DefaultUsersAdminListFilterProvider : IUsersAdminListFilterProvider
    {
        public void Build(QueryEngineBuilder<User> builder)
        {
            builder
                .WithNamedTerm("status", builder => builder
                    .OneCondition((val, query) =>
                    {
                        if (Enum.TryParse<UsersFilter>(val, true, out var usersStatus))
                        {
                            switch (usersStatus)
                            {
                                case UsersFilter.Enabled:
                                    query.With<UserIndex>(u => u.IsEnabled);
                                    break;
                                case UsersFilter.Disabled:
                                    query.With<UserIndex>(u => !u.IsEnabled);
                                    break;
                            }
                        }

                        return query;
                    })
                    .MapTo<UserIndexOptions>((val, model) =>
                    {
                        if (Enum.TryParse<UsersFilter>(val, true, out var usersFilter))
                        {
                            model.Filter = usersFilter;
                        }
                    })
                    .MapFrom<UserIndexOptions>((model) =>
                    {
                        return model.Filter switch
                        {
                            UsersFilter.Enabled => (true, model.Filter.ToString()),
                            UsersFilter.Disabled => (true, model.Filter.ToString()),
                            _ => (false, String.Empty)
                        };
                    })
                )
                .WithNamedTerm("sort", builder => builder
                    .OneCondition((val, query) =>
                    {
                        if (Enum.TryParse<UsersOrder>(val, true, out var usersOrder))
                        {
                            switch (usersOrder)
                            {
                                case UsersOrder.Name:
                                    query.With<UserIndex>().OrderBy(u => u.NormalizedUserName);
                                    break;
                                case UsersOrder.Email:
                                    query.With<UserIndex>().OrderBy(u => u.NormalizedEmail);
                                    break;
                            };
                        }
                        else
                        {
                            query.With<UserIndex>().OrderBy(u => u.NormalizedUserName);
                        }

                        return query;
                    })
                    .MapTo<UserIndexOptions>((val, model) =>
                    {
                        if (Enum.TryParse<UsersOrder>(val, true, out var order))
                        {
                            model.Order = order;
                        }
                    })
                    .MapFrom<UserIndexOptions>((model) =>
                    {
                        if (model.Order != UsersOrder.Name)
                        {
                            return (true, model.Order.ToString());
                        }

                        return (false, String.Empty);
                    })
                    .AlwaysRun()
                )
                .WithNamedTerm("role", builder => builder
                    .OneCondition((val, query, ctx) =>
                    {
                        var context = (UserQueryContext)ctx;
                        var userManager = context.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                        var normalizedRoleName = userManager.NormalizeName(val);
                        query.With<UserByRoleNameIndex>(x => x.RoleName == normalizedRoleName);

                        return new ValueTask<IQuery<User>>(query);
                    })
                    .MapTo<UserIndexOptions>((val, model) => model.SelectedRole = val)
                    .MapFrom<UserIndexOptions>((model) => (!String.IsNullOrEmpty(model.SelectedRole), model.SelectedRole))
                )
                .WithDefaultTerm(UsersAdminListFilterOptions.DefaultTermName, builder => builder
                    .ManyCondition(
                        (val, query, ctx) =>
                        {
                            var context = (UserQueryContext)ctx;
                            var userManager = context.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                            var normalizedUserName = userManager.NormalizeName(val);
                            query.With<UserIndex>(x => x.NormalizedUserName.Contains(normalizedUserName));

                            return new ValueTask<IQuery<User>>(query);
                        },
                        (val, query, ctx) =>
                        {
                            var context = (UserQueryContext)ctx;
                            var userManager = context.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                            var normalizedUserName = userManager.NormalizeName(val);
                            query.With<UserIndex>(x => x.NormalizedUserName.NotContains(normalizedUserName));

                            return new ValueTask<IQuery<User>>(query);
                        }
                    )
                )
                .WithNamedTerm("email", builder => builder
                    .ManyCondition(
                        (val, query, ctx) =>
                        {
                            var context = (UserQueryContext)ctx;
                            var userManager = context.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                            var normalizedEmail = userManager.NormalizeEmail(val);
                            query.With<UserIndex>(x => x.NormalizedEmail.Contains(normalizedEmail));

                            return new ValueTask<IQuery<User>>(query);
                        },
                        (val, query, ctx) =>
                        {
                            var context = (UserQueryContext)ctx;
                            var userManager = context.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                            var normalizedEmail = userManager.NormalizeEmail(val);
                            query.With<UserIndex>(x => x.NormalizedEmail.NotContains(normalizedEmail));

                            return new ValueTask<IQuery<User>>(query);
                        }
                    )
                );

        }
    }
}
