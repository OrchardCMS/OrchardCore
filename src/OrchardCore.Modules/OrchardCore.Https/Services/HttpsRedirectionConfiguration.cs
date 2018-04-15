using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.HttpsPolicy;

namespace OrchardCore.Https.Services
{
    public class HttpsRedirectionConfiguration : IConfigureOptions<HttpsRedirectionOptions>
    {
        private readonly IHttpsService _httpsService;

        public HttpsRedirectionConfiguration(IHttpsService httpsService)
        {
            _httpsService = httpsService;
        }

        public void Configure(HttpsRedirectionOptions options)
        {
            var httpsSettings = _httpsService.GetSettingsAsync().GetAwaiter().GetResult();

            options.RedirectStatusCode = httpsSettings.RequireHttpsPermanent
                ? StatusCodes.Status301MovedPermanently
                : StatusCodes.Status302Found;
            options.HttpsPort = httpsSettings.SslPort;
        }
    }
}