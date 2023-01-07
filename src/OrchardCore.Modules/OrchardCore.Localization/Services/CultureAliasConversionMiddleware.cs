using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Localization.Services
{
    public class CultureAliasConversionMiddleware
    {
        private readonly ICultureAliasProvider _cultureAliasProvider;
        private readonly RequestDelegate _next;

        public CultureAliasConversionMiddleware(ICultureAliasProvider cultureAliasProvider, RequestDelegate next)
        {
            _cultureAliasProvider = cultureAliasProvider;
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            if (_cultureAliasProvider.TryGetCulture(CultureInfo.CurrentCulture, out var culture))
            {
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }

            return _next.Invoke(context);
        }
    }
}
