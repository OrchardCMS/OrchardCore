using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Scripting;

namespace OrchardCore.Workflows.Http.Scripting;

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
    private readonly GlobalMethod _deserializeRequestDataMethod;

    public HttpMethodsProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextMethod = new GlobalMethod
        {
            Name = "httpContext",
            Method = serviceProvider => (Func<HttpContext>)(() => httpContextAccessor.HttpContext),
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
            }),
        };

        _responseWriteMethod = new GlobalMethod
        {
            Name = "responseWrite",
            Method = serviceProvider => (Action<string>)(text =>
            {
                httpContextAccessor.HttpContext.Items[WorkflowHttpResult.Instance] = WorkflowHttpResult.Instance;
                httpContextAccessor.HttpContext.Response.WriteAsync(text).GetAwaiter().GetResult();
            })
        };

        _absoluteUrlMethod = new GlobalMethod
        {
            Name = "absoluteUrl",
            Method = serviceProvider => (Func<string, string>)(relativePath =>
            {
                var urlHelperFactory = serviceProvider.GetRequiredService<IUrlHelperFactory>();
                var urlHelper = urlHelperFactory.GetUrlHelper(new ActionContext(httpContextAccessor.HttpContext, new RouteData(), new ActionDescriptor()));
                return urlHelper.ToAbsoluteUrl(relativePath);
            }),
        };

        _readBodyMethod = new GlobalMethod
        {
            Name = "readBody",
            Method = serviceProvider => (Func<string>)(() =>
            {
                using var sr = new StreamReader(httpContextAccessor.HttpContext.Request.Body);

                // Async read of the request body is mandatory.
                return sr.ReadToEndAsync().GetAwaiter().GetResult();
            }),
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
            }),
        };

        // This should be deprecated
        _queryStringAsJsonMethod = new GlobalMethod
        {
            Name = "queryStringAsJson",
            Method = serviceProvider => _deserializeRequestDataMethod.Method.Invoke(serviceProvider),
        };

        // This should be deprecated
        _requestFormAsJsonMethod = new GlobalMethod
        {
            Name = "requestFormAsJson",
            Method = serviceProvider => _deserializeRequestDataMethod.Method.Invoke(serviceProvider),
        };

        _deserializeRequestDataMethod = new GlobalMethod
        {
            Name = "deserializeRequestData",
            Method = serviceProvider => (Func<Dictionary<string, object>>)(() =>
            {
                Dictionary<string, object> result = null;

                if (httpContextAccessor.HttpContext != null)
                {
                    var method = httpContextAccessor.HttpContext.Request.Method;
                    if (method.Equals("POST", StringComparison.OrdinalIgnoreCase) || method.Equals("PUT", StringComparison.OrdinalIgnoreCase) || method.Equals("PATCH", StringComparison.OrdinalIgnoreCase))
                    {
                        if (httpContextAccessor.HttpContext.Request.HasFormContentType)
                        {
                            var formData = httpContextAccessor.HttpContext.Request.Form;

                            // If we can parse first request form element key as JSON then we throw
                            if (isValidJSON(formData.First().Key.ToString()))
                            {
                                throw new Exception("Invalid form data passed in the request. The data passed was JSON while it should be form data.");
                            }

                            try
                            {
                                result = formData.ToDictionary(x => x.Key, x => (object)x.Value);
                            }
                            catch
                            {
                                throw new Exception("Invalid form data passed in the request.");
                            }
                        }
                        else if (httpContextAccessor.HttpContext.Request.HasJsonContentType())
                        {
                            string json;
                            using (var sr = new StreamReader(httpContextAccessor.HttpContext.Request.Body))
                            {
                                // Async read of the request body is mandatory.
                                json = sr.ReadToEndAsync().GetAwaiter().GetResult();
                            }

                            try
                            {
                                result = JConvert.DeserializeObject<Dictionary<string, object>>(json);
                            }
                            catch
                            {
                                throw new Exception("Invalid JSON passed in the request.");
                            }
                        }
                    }
                    else if (httpContextAccessor.HttpContext.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                    {
                        var queryData = httpContextAccessor.HttpContext.Request.Query;

                        try
                        {
                            result = queryData.ToDictionary(x => x.Key, x => (object)x.Value);

                            // We never need to keep the Workflow token
                            result.Remove("token");
                        }
                        catch
                        {
                            throw new Exception("Invalid query string data passed in the request.");
                        }
                    }
                    else
                    {
                        throw new Exception("The request method is not supported");
                    }
                }

                return result;
            }),
        };
    }

    private static bool isValidJSON(string json)
    {
        try
        {
            var token = JObject.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public IEnumerable<GlobalMethod> GetMethods()
    {
        return new[] { _httpContextMethod, _queryStringMethod, _responseWriteMethod, _absoluteUrlMethod, _readBodyMethod, _requestFormMethod, _queryStringAsJsonMethod, _requestFormAsJsonMethod, _deserializeRequestDataMethod };
    }
}
