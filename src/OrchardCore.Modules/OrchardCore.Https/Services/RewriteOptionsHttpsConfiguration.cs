using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;

namespace OrchardCore.Https.Services
{
    public class RewriteOptionsHttpsConfiguration : IConfigureOptions<RewriteOptions>
    {
        private readonly IHttpsService _httpsService;

        public RewriteOptionsHttpsConfiguration(IHttpsService httpsService)
        {
            _httpsService = httpsService;
        }

        public void Configure(RewriteOptions options)
        {
            var httpsSettings = _httpsService.GetSettingsAsync().GetAwaiter().GetResult();

            if (httpsSettings.RequireHttps)
            {
                options.AddRedirectToHttps(
                    httpsSettings.RequireHttpsPermanent
                        ? StatusCodes.Status301MovedPermanently
                        : StatusCodes.Status302Found, 
                    httpsSettings.SslPort);
            }
        }
    }
}