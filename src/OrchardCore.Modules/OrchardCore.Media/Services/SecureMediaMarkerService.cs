using System;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Media.Services
{
    internal class SecureMediaMarkerService { }

    internal static class SecureMediaExtensions
    {
        public static bool IsSecureMediaEnabled(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService(typeof(SecureMediaMarkerService)) is not null;
        }

        public static bool IsSecureMediaEnabled(this HttpContext httpContext)
        {
            return httpContext.RequestServices.IsSecureMediaEnabled();
        }
    }
}
