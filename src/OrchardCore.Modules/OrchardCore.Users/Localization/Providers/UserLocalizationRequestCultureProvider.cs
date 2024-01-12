using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace OrchardCore.Users.Localization.Providers;

public class UserLocalizationRequestCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (httpContext?.User?.Identity?.IsAuthenticated == false)
        {
            return NullProviderCultureResult;
        }

        string userCulture = null;
        var claimCulture = httpContext.User.FindFirstValue("culture");

        if (!string.IsNullOrWhiteSpace(claimCulture))
        {
            userCulture = claimCulture;
        }

        if (userCulture == null)
        {
            return NullProviderCultureResult;
        }

        return Task.FromResult(new ProviderCultureResult(userCulture));
    }
}
