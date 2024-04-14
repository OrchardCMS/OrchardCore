using System.Security.Claims;

namespace OrchardCore.Media
{
    public interface IUserAssetFolderNameProvider
    {
        string GetUserAssetFolderName(ClaimsPrincipal claimsPrincipal);
    }
}
