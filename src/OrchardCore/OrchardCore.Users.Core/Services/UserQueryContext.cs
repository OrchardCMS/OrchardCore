using System;
using OrchardCore.Users.Models;
using YesSql;
using YesSql.Filters.Query.Services;

namespace OrchardCore.Users.Services
{
    public class UserQueryContext : QueryExecutionContext<User>
    {
        public UserQueryContext(IServiceProvider serviceProvider, IQuery<User> query) : base(query)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }
    }
}
