using System.Security.Claims;

namespace OrchardCore.Media.Services
{
    public class DefaultUserAssetFolderNameProvider : IUserAssetFolderNameProvider
    {
        public string GetUserAssetFolderName(ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
