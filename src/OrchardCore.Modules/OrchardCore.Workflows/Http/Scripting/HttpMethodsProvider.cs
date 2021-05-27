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
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Scripting;
using OrchardCore.Infrastructure.Html;

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
        private readonly GlobalMethod _requestDataAsJsonObjectMethod;

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
                    var urlHelperFactory = serviceProvider.GetRequiredService<IUrlHelperFactory>();
                    var urlHelper = urlHelperFactory.GetUrlHelper(new ActionContext(httpContextAccessor.HttpContext, new RouteData(), new ActionDescriptor()));
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

            // This should be deprecated
            _requestFormAsJsonMethod = new GlobalMethod
            {
                Name = "requestFormAsJson",
                Method = serviceProvider => (Func<JObject>)(() =>
                    new JObject((from field in httpContextAccessor.HttpContext.Request.Form
                                 select new JProperty(field.Key, JArray.FromObject(field.Value.ToArray()))).ToArray()))
            };

            _requestDataAsJsonObjectMethod = new GlobalMethod
            {
                Name = "requestDataAsJsonObject",
                Method = serviceProvider => (Func<JObject>)(() =>
                {
                    JObject result = null;
                    var sanitizer = serviceProvider.GetRequiredService<IHtmlSanitizerService>();

                    if(httpContextAccessor.HttpContext != null)
                    {
                        if (httpContextAccessor.HttpContext.Request.HasFormContentType)
                        {
                            try
                            {
                                result = new JObject(httpContextAccessor.HttpContext.Request.Form.Select(
                                field =>
                                {
                                    var arr = field.Value.ToArray();
                                    if (arr.Length == 1)
                                    {
                                        return new JProperty(field.Key, sanitizer.Sanitize(field.Value[0]));
                                    }
                                    return new JProperty(field.Key, JArray.FromObject(arr.Select(o => sanitizer.Sanitize(o))));
                                }
                                ).ToArray());
                            }
                            catch
                            {
                                throw new Exception("Invalid form data passed in the request.");
                            }
                        }
                        else if (HasJsonContentType(httpContextAccessor.HttpContext.Request))
                        {
                            string json;
                            using (var sr = new StreamReader(httpContextAccessor.HttpContext.Request.Body))
                            {
                                // Async read of the request body is mandatory.
                                json = sanitizer.Sanitize(sr.ReadToEndAsync().GetAwaiter().GetResult());
                            }

                            try
                            {
                                result = JObject.Parse(json);
                            }
                            catch
                            {
                                throw new Exception("Invalid JSON passed in the request.");
                            }
                        }
                    }

                    return result;
                })
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { _httpContextMethod, _queryStringMethod, _responseWriteMethod, _absoluteUrlMethod, _readBodyMethod, _requestFormMethod, _queryStringAsJsonMethod, _requestFormAsJsonMethod, _requestDataAsJsonObjectMethod };
        }

        /// <summary>
        /// Checks the Content-Type header for JSON types.
        /// This method needs to be removed after we drop support for netcoreapp3.1
        /// It is now part of net5.0 see :
        /// https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httprequestjsonextensions.hasjsoncontenttype?view=aspnetcore-5.0
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static bool HasJsonContentType(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var mt))
            {
                return false;
            }

            // Matches application/json
            if (mt.MediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Matches +json, e.g. application/ld+json
            if (mt.Suffix.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
