using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Scripting;

namespace OrchardCore.Workflows.Http.Scripting
{
    // TODO: Consider moving this to a more common package so that it's available without a dependency on Workflows.
    public class HttpMethodsProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _httpContextMethod;
        private readonly GlobalMethod _queryStringMethod;
        private readonly GlobalMethod _responseWriteMethod;
        private readonly GlobalMethod _absoluteUrlMethod;
        private readonly GlobalMethod _readBodyMethod;
        private readonly GlobalMethod _requestFormMethod;
        private readonly GlobalMethod _queryStringAsJsonMethod;
        private readonly GlobalMethod _requestFormAsJsonMethod;

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
                Method = serviceProvider => (Func<string, object>)(name =>
                {
                    if (name == null)
                    {
                        return httpContextAccessor.HttpContext.Request.QueryString.ToString();
                    }
                    object result;
                    if (httpContextAccessor.HttpContext.Request.Query.TryGetValue(name, out var values))
                    {
                        if (values.Count == 0)
                        {
                            result = null;
                        }
                        else if (values.Count == 1)
                        {
                            result = values[0];
                        }
                        else
                        {
                            result = values.ToArray();
                        }
                    }
                    else
                    {
                        result = null;
                    }
                    return result;
                })
            };

            _responseWriteMethod = new GlobalMethod
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
                    using (var sr = new StreamReader(httpContextAccessor.HttpContext.Request.Body))
                    {
                        // Async read of the request body is mandatory.
                        return sr.ReadToEndAsync().GetAwaiter().GetResult();
                    }
                })
            };

            _requestFormMethod = new GlobalMethod
            {
                Name = "requestForm",
                Method = serviceProvider => (Func<string, object>)(field =>
                {
                    object result;
                    if (httpContextAccessor.HttpContext.Request.Form.TryGetValue(field, out var values))
                    {
                        if (values.Count == 0)
                        {
                            result = null;
                        }
                        else if (values.Count == 1)
                        {
                            result = values[0];
                        }
                        else
                        {
                            result = values.ToArray();
                        }
                    }
                    else
                    {
                        result = null;
                    }
                    return result;
                })
            };

            _queryStringAsJsonMethod = new GlobalMethod
            {
                Name = "queryStringAsJson",
                Method = serviceProvider => (Func<JObject>)(() =>
                    new JObject((from param in httpContextAccessor.HttpContext.Request.Query
                                 select new JProperty(param.Key, JArray.FromObject(param.Value.ToArray()))).ToArray()))
            };

            _requestFormAsJsonMethod = new GlobalMethod
            {
                Name = "requestFormAsJson",
                Method = serviceProvider => (Func<JObject>)(() =>
                    new JObject((from field in httpContextAccessor.HttpContext.Request.Form
                                 select new JProperty(field.Key, JArray.FromObject(field.Value.ToArray()))).ToArray()))
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { _httpContextMethod, _queryStringMethod, _responseWriteMethod, _absoluteUrlMethod, _readBodyMethod, _requestFormMethod, _queryStringAsJsonMethod, _requestFormAsJsonMethod };
        }
    }
}
