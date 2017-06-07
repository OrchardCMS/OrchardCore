using System;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Settings;

namespace Orchard.Tokens
{
    public static class StandardTokens
    {
        public static void RegisterStandardTokens(this IHandlebars handlebars, IHttpContextAccessor httpContextAccessor)
        {
            handlebars.RegisterHelper("dateformat", (output, context, arguments) =>
            {
                var services = httpContextAccessor.HttpContext.RequestServices;
                var clock = services.GetRequiredService<IClock>();
                var siteService = services.GetRequiredService<ISiteService>();
                var site = siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(site.TimeZone);
                var now = TimeZoneInfo.ConvertTime(clock.UtcNow, TimeZoneInfo.Utc, timeZone);

                var format = arguments[0].ToString();
                output.Write(now.ToString(format));
            });
        }
    }
}
