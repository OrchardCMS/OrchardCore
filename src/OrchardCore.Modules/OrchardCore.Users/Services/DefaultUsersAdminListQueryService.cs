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
    public class DefaultContentsAdminListQueryService : IUsersAdminListQueryService
    {
        private readonly ISession _session;
        private readonly IEnumerable<IUsersAdminListFilter> _usersAdminListFilters;
        private readonly ILogger _logger;

        public DefaultContentsAdminListQueryService(
            ISession session,
            IEnumerable<IUsersAdminListFilter> usersAdminListFilters,
            ILogger<DefaultContentsAdminListQueryService> logger)
        {
            _session = session;
            _usersAdminListFilters = usersAdminListFilters;
            _logger = logger;
        }

        public async Task<IQuery<User>> QueryAsync(UserIndexOptions options, IUpdateModel updater)
        {
            // Because admin filters can add a different index to the query this must be added as a Query<User>()
            var query = _session.Query<User>();

            await _usersAdminListFilters.InvokeAsync((filter, options, query, updater) => filter.FilterAsync(options, query, updater), options, query, updater, _logger);

            return query;
        }
    }
}
