using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;
using YesSql;

namespace OrchardCore.Users;

public interface IUsersAdminListFilter
{
    Task FilterAsync(UserIndexOptions model, IQuery<User> query, IUpdateModel updater);
}
