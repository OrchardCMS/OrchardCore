using System;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Media.Services
{
    internal sealed class SecureMediaMarkerService { }

    internal static class SecureMediaExtensions
    {
        private const string IsSecureMediaKey = "IsSecureMedia";

        public static bool IsSecureMediaEnabled(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService(typeof(SecureMediaMarkerService)) is not null;
        }

        public static bool IsSecureMediaEnabled(this HttpContext httpContext)
        {
            return httpContext.RequestServices.IsSecureMediaEnabled();
        }

        public static bool IsSecureMediaRequested(this HttpContext httpContext)
        {
            return httpContext.Items.ContainsKey(IsSecureMediaKey);
        }

        public static void MarkAsSecureMediaRequested(this HttpContext httpContext)
        {
            httpContext.Items[IsSecureMediaKey] = bool.TrueString;
        }
    }
}
