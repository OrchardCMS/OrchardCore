using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace OrchardCore.Https.Services
{
    public class MvcOptionsHttpsConfiguration : IConfigureOptions<MvcOptions>
    {
        private readonly IHttpsService _sslService;

        public MvcOptionsHttpsConfiguration(IHttpsService sslService)
        {
            _sslService = sslService;
        }

        public void Configure(MvcOptions options)
        {
            var sslSettings = _sslService.GetSettingsAsync().GetAwaiter().GetResult();

            if (sslSettings.RequireHttps)
            {
                options.Filters.Add(new RequireHttpsAttribute());
                options.RequireHttpsPermanent = sslSettings.RequireHttpsPermanent;
                options.SslPort = sslSettings.SslPort;
            }
        }
    }
}