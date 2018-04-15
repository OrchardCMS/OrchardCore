using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Security.AuthorizationHandlers;
using OrchardCore.Security.SecurityHeaders;

namespace OrchardCore.Security.SecurityHeaders
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default security headers.
        /// </summary>
        public static IServiceCollection AddSecurityHeaders(this IServiceCollection services)
        {
            // Configure the default security header options
            services.AddReferrerPolicy(ReferrerPolicy.NoReferrer);
            services.AddXssProtection(XssProtectionPolicy.FilterAndBlock);
            services.AddXFrameDeny();
            services.AddContentTypeOptionsNoSniff();
			
            // Microsoft might add support for the Content-Security-Policy header so leaving that for them.
            // https://github.com/aspnet/BasicMiddleware/issues/259
            // services.AddCsp();

            return services;
        }

        /// <summary>
        /// Adds a Referrer-Policy header to all the server's responses.
        /// </summary>
        /// <param name="policy">The referrer policy that determines the header's value.</param>
        /// <returns></returns>
        public static IServiceCollection AddReferrerPolicy(this IServiceCollection services, ReferrerPolicy policy)
        {
            services.AddSecurityHeader("Referrer-Policy", GetReferrerPolicyHeaderValue(policy));

            return services;
        }

        /// <summary>
        /// Adds a X-XSS-Protection header to all the server's responses.
        /// </summary>
        /// <param name="policy">The XSS protection policy that determines the header's value.</param>
        /// <returns></returns>
        public static IServiceCollection AddXssProtection(this IServiceCollection services, XssProtectionPolicy policy)
        {
            services.AddSecurityHeader("X-XSS-Protection", GetXssProtectionPolicyHeaderValue(policy));

            return services;
        }

        /// <summary>
        /// Adds a X-Frame-Options deny header to all the server's responses.
        /// </summary>
        public static IServiceCollection AddXFrameDeny(this IServiceCollection services)
        {
            services.AddSecurityHeader("X-Frame-Options", "DENY");

            return services;
        }

        /// <summary>
        /// Adds a X-Frame-Options deny header to all the server's responses.
        /// </summary>
        public static IServiceCollection AddXFrameSameOrigin(this IServiceCollection services)
        {
            services.AddSecurityHeader("X-Frame-Options", "SAMEORIGIN");

            return services;
        }

        /// <summary>
        /// Adds a X-Frame-Options allow-from header to all the server's responses.
        /// </summary>
        /// <param name="uri">The allowed uri from which the site can be framed.</param>
        public static IServiceCollection AddXFrameAllowFrom(this IServiceCollection services, string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentNullException(nameof(uri));
            }

            services.AddSecurityHeader("X-Frame-Options", "ALLOW-FROM " + uri);

            return services;
        }

        /// <summary>
        /// Adds a X-Content-Type-Options header with a value of no-sniff to all the server's responses.
        /// </summary>
        /// <param name="uri">The allowed uri from which the site can be framed.</param>
        public static IServiceCollection AddContentTypeOptionsNoSniff(this IServiceCollection services)
        {
            services.AddSecurityHeader("X-Content-Type-Options", "no-sniff");

            return services;
        }

        /// <summary>
        /// Adds the security header to all server responses.
        /// </summary>
        /// <param name="name">The header name to be added.</param>
        /// <param name="value">The header value.</param>
        public static IServiceCollection AddSecurityHeader(this IServiceCollection services, string name, string value)
        {
            services.Configure<SecurityHeaderOptions>(options => options.AddHeaders[name] = value);

            return services;
        }

        /// <summary>
        /// Removes the security header from all server responses.
        /// </summary>
        /// <param name="name">The name of the header to be removed.</param>
        public static IServiceCollection RemoveSecurityHeader(this IServiceCollection services, string name)
        {
            services.Configure<SecurityHeaderOptions>(options => options.AddHeaders.Remove(name));

            return services;
        }

        private static string GetXssProtectionPolicyHeaderValue(XssProtectionPolicy xssProtectionPolicy)
        {
            switch (xssProtectionPolicy)
            {
                case XssProtectionPolicy.DisableFiltering:
                    return "0";
                case XssProtectionPolicy.FilterAndSanitize:
                    return "1";
                case XssProtectionPolicy.FilterAndBlock:
                    return "1; mode=block";
                default:
                    throw new ArgumentOutOfRangeException(nameof(xssProtectionPolicy));
            }
        }

        private static string GetReferrerPolicyHeaderValue(ReferrerPolicy referrerPolicy)
        {
            switch (referrerPolicy)
            {
                case ReferrerPolicy.NoReferrer:
                    return "no-referrer";
                case ReferrerPolicy.NoReferrerWhenDowngrade:
                    return "no-referrer-when-downgrade";
                case ReferrerPolicy.Origin:
                    return "origin";
                case ReferrerPolicy.OriginWhenCrossOrigin:
                    return "origin-when-cross-origin";
                case ReferrerPolicy.SameOrigin:
                    return "same-origin";
                case ReferrerPolicy.StrictOrigin:
                    return "strict-origin";
                case ReferrerPolicy.StrictOriginWhenCrossOrigin:
                    return "strict-origin-when-cross-origin";
                case ReferrerPolicy.UnsafeUrl:
                    return "unsafe-url";
                default:
                    throw new ArgumentOutOfRangeException(nameof(referrerPolicy));
            }
        }
    }
}