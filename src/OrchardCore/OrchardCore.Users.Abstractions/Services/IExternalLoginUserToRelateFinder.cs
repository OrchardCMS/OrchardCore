
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace OrchardCore.Users.Abstractions;

public interface IExternalLoginUserToRelateFinder
{
  bool CanManageThis(string extLoginKind);

  Task<IUser> FindUserToRelateAsync(ExternalLoginInfo info);

  string GetValueThatLinkAccount(ExternalLoginInfo info);
}