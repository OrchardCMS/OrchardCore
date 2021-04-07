using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Data;
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
        private readonly IEnumerable<IUsersAdminListFilter> _usersAdminListFilters;
        private readonly ILogger _logger;

        private string userCollection = "User";
        IOptions<StoreCollectionOptions> _storeCollections;

        public DefaultUsersAdminListQueryService(
            ISession session,
            IEnumerable<IUsersAdminListFilter> usersAdminListFilters,
            ILogger<DefaultUsersAdminListQueryService> logger,
            IOptions<StoreCollectionOptions> storeCollections)
        {
            _session = session;
            _usersAdminListFilters = usersAdminListFilters;
            _logger = logger;
            _storeCollections = storeCollections;
            userCollection = storeCollections.Value.Collections["OrchardCore.Users"] ?? "";
        }

        public async Task<IQuery<User>> QueryAsync(UserIndexOptions options, IUpdateModel updater)
        {
            // Because admin filters can add a different index to the query this must be added as a Query<User>()
            var query = _session.Query<User>(userCollection);

            await _usersAdminListFilters.InvokeAsync((filter, options, query, updater) => filter.FilterAsync(options, query, updater), options, query, updater, _logger);

            return query;
        }
    }
}
