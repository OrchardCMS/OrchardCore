using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Localization.Services
{
    public class CultureAliasConversionMiddleware
    {
        private readonly ICultureAliasProvider _cultureAliasProvider;
        private readonly RequestLocalizationOptions _requestLocalizationOptions;
        private readonly RequestDelegate _next;

        public CultureAliasConversionMiddleware(
            ICultureAliasProvider cultureAliasProvider,
            IOptions<RequestLocalizationOptions> requestLocalizationOptions,
            RequestDelegate next)
        {
            _cultureAliasProvider = cultureAliasProvider;
            _requestLocalizationOptions = requestLocalizationOptions.Value;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var requestCultureFeature = context.Features.Get<IRequestCultureFeature>();

            if (requestCultureFeature.Provider == null && _requestLocalizationOptions.RequestCultureProviders != null)
            {
                var cultureInfo = requestCultureFeature.RequestCulture.Culture;

                foreach (var provider in _requestLocalizationOptions.RequestCultureProviders)
                {
                    var providerResultCulture = await provider.DetermineProviderCultureResult(context);

                    if (providerResultCulture == null)
                    {
                        continue;
                    }

                    cultureInfo = GetCultureInfo(providerResultCulture.Cultures);

                    if (cultureInfo == null)
                    {
                        continue;
                    }
                }

                if (_cultureAliasProvider.TryGetCulture(cultureInfo, out var culture))
                {
                    CultureInfo.CurrentCulture = culture;
                    CultureInfo.CurrentUICulture = culture;
                }
            }

            await _next.Invoke(context);
        }

        private static CultureInfo GetCultureInfo(IEnumerable<StringSegment> cultureNames)
        {
            CultureInfo cultureInfo = null;
            foreach (var cultureName in cultureNames)
            {
                if (cultureName != null)
                {
                    cultureInfo = CultureInfo.GetCultureInfo(cultureName.Value);

                    if (cultureInfo != null)
                    {
                        break;
                    }
                }
            }

            return cultureInfo;
        }
    }
}
