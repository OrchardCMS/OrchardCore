using OrchardCore.Filters.Core;
using OrchardCore.Users.Models;
using YesSql.Filters.Query;

namespace OrchardCore.Users.Services
{
    public class UserFilterEngineModelBinder : FilterEngineModelBinder<User>
    {
        public UserFilterEngineModelBinder(IQueryParser<User> parser) : base(parser)
        {
        }
    }
}
