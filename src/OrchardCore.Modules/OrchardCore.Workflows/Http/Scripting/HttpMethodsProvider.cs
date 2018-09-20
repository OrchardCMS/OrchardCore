using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Scripting;

namespace OrchardCore.Workflows.Http.Scripting
{
    // TODO: Consider moving this to a more common package so that it's available without a dependency on Workflows.
    public class HttpMethodsProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _httpContextMethod;
        private readonly GlobalMethod _queryStringMethod;
        private readonly GlobalMethod _writeMethod;
        private readonly GlobalMethod _absoluteUrlMethod;
        private readonly GlobalMethod _readBodyMethod;

        public HttpMethodsProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextMethod = new GlobalMethod
            {
                Name = "httpContext",
                Method = serviceProvider => (Func<HttpContext>)(() => httpContextAccessor.HttpContext)
            };

            _queryStringMethod = new GlobalMethod
            {
                Name = "queryString",
                Method = serviceProvider => (Func<string, string>)(name => httpContextAccessor.HttpContext.Request.Query[name].ToString())
            };

            _writeMethod = new GlobalMethod
            {
                Name = "responseWrite",
                Method = serviceProvider => (Action<string>)(text => httpContextAccessor.HttpContext.Response.WriteAsync(text).GetAwaiter().GetResult())
            };

            _absoluteUrlMethod = new GlobalMethod
            {
                Name = "absoluteUrl",
                Method = serviceProvider => (Func<string, string>)(relativePath =>
                {
                    var urlHelperّFactory = serviceProvider.GetRequiredService<IUrlHelperFactory>();
                    var urlHelper = urlHelperّFactory.GetUrlHelper(new ActionContext(httpContextAccessor.HttpContext, new RouteData(), new ActionDescriptor()));
                    return urlHelper.ToAbsoluteUrl(relativePath);
                })
            };

            _readBodyMethod = new GlobalMethod
            {
                Name = "readBody",
                Method = serviceProvider => (Func<string>)(() =>
                {
                    var stream = httpContextAccessor.HttpContext.Request.Body;
                    var body = new StreamReader(stream).ReadToEnd();
                    return body;
                })
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { _httpContextMethod, _queryStringMethod, _writeMethod, _absoluteUrlMethod, _readBodyMethod };
        }
    }
}
