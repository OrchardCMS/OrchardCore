using OrchardCore.Filters.Core;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services
{
    public class UserFilterEngineModelBinder : FilterEngineModelBinder<User>
    {
        public UserFilterEngineModelBinder(IUsersAdminListFilterParser parser) : base(parser)
        {
        }
    }
}
