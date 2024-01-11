using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;
using YesSql;

namespace OrchardCore.Users.Services
{
    public interface IUsersAdminListQueryService
    {
        Task<IQuery<User>> QueryAsync(UserIndexOptions options, IUpdateModel updater);
    }
}
