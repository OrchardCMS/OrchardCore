using System.Security.Claims;
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

        var userCulture = httpContext.User.GetCulture(); // String.Empty here means that it did not find the Culture Claim.

        if (String.IsNullOrWhiteSpace(userCulture))
        {
            return NullProviderCultureResult;
        }

        return Task.FromResult(new ProviderCultureResult(userCulture == UserLocalizationConstants.Invariant ? "" : userCulture));
    }
}
