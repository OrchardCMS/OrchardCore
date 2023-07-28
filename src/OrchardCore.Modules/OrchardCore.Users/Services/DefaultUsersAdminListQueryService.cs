using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;
using YesSql;

namespace OrchardCore.Users.Services
{
    public class DefaultUsersAdminListQueryService : IUsersAdminListQueryService
    {
        private readonly ISession _session;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly IEnumerable<IUsersAdminListFilter> _usersAdminListFilters;

        public DefaultUsersAdminListQueryService(
            ISession session,
            IServiceProvider serviceProvider,
            ILogger<DefaultUsersAdminListQueryService> logger,
            IEnumerable<IUsersAdminListFilter> usersAdminListFilters)
        {
            _session = session;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _usersAdminListFilters = usersAdminListFilters;
        }

        public async Task<IQuery<User>> QueryAsync(UserIndexOptions options, IUpdateModel updater)
        {
            // Because admin filters can add a different index to the query this must be added as a Query<User>()
            var query = _session.Query<User>();

            query = await options.FilterResult.ExecuteAsync(new UserQueryContext(_serviceProvider, query));

            await _usersAdminListFilters.InvokeAsync((filter, model, query, updater) => filter.FilterAsync(model, query, updater), options, query, updater, _logger);

            return query;
        }
    }
}
