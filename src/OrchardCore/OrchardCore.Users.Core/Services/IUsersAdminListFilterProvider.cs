using OrchardCore.Users.Models;
using YesSql.Filters.Query;

namespace OrchardCore.Users.Services
{
    public interface IUsersAdminListFilterProvider
    {
        void Build(QueryEngineBuilder<User> builder);
    }
}
