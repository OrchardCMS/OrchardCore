using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Localization;

namespace OrchardCore.Users.Localization.Providers;

public class UserLocalizationRequestCultureProvider : RequestCultureProvider
{
    public override async Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (httpContext?.User?.Identity?.IsAuthenticated == false)
        {
            return await NullProviderCultureResult;
        }

        string userCulture = null;
        var claimCulture = httpContext.User.FindFirstValue("culture");

        if (!String.IsNullOrWhiteSpace(claimCulture))
        {
            userCulture = claimCulture;
        }

        if (userCulture == null)
        {
            // No values specified for either so no match
            return await NullProviderCultureResult;
        }

        var localizationService = httpContext.RequestServices.GetService<ILocalizationService>();

        var supportedCulture = await localizationService.GetSupportedCulturesAsync();

        // We verify that the userCulture is still a supportedCulture
        if (!supportedCulture.Contains(userCulture))
        {
            return await NullProviderCultureResult;
        }

        var requestCulture = new ProviderCultureResult(userCulture);

        return requestCulture;
    }
}
