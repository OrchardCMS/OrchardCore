using System;
using OrchardCore.Admin;

namespace OrchardCore.Seo;

public class SeoHelpers
{
    public static string GetDefaultRobotsContents(AdminOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return $"User-agent: *\r\nAllow: /\r\nDisallow: /{options.AdminUrlPrefix}";
    }
}
