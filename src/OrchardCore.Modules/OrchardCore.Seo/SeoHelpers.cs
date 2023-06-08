using OrchardCore.Admin;

namespace OrchardCore.Seo;

public class SeoHelpers
{
    public static string GetDefaultRobotsContents(AdminOptions adminOptions)
    {
        return $"User-agent: *\r\nAllow: /\r\nDisallow: /{adminOptions.AdminUrlPrefix}";
    }
}
