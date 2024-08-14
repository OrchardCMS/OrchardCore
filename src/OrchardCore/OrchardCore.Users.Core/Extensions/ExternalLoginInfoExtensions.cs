using System.Security.Claims;

namespace Microsoft.AspNetCore.Identity;

public static class ExternalLoginInfoExtensions
{
    public static string GetEmail(this ExternalLoginInfo info)
        => info.Principal.FindFirstValue(ClaimTypes.Email) ?? info.Principal.FindFirstValue("email");
}
