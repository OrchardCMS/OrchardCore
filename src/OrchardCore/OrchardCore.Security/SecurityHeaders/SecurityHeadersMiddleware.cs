using System;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;

namespace OrchardCore.Security.SecurityHeaders
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SecurityHeaderOptions _headers;

        public SecurityHeadersMiddleware(RequestDelegate next, IOptions<SecurityHeaderOptions> headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            _next = next ?? throw new ArgumentNullException(nameof(next));

            _headers = headers.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var response = context.Response;

            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            var headers = response.Headers;

            foreach (var header in _headers.AddHeaders)
            {
                headers[header.Key] = header.Value;
            }

            await _next(context);
        }
    }
}
