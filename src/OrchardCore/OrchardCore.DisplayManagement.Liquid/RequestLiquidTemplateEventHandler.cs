using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid
{
    /// <summary>
    /// Provides access to <see cref="HttpRequest"/> properties if the template is running in
    /// a web request.
    /// </summary>
    public class RequestLiquidTemplateEventHandler : ILiquidTemplateEventHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private HttpContext _httpContext;

        static RequestLiquidTemplateEventHandler()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<HttpRequest, FluidValue>((request, name) =>
            {
                switch (name)
                {
                    case "QueryString": return new StringValue(request.QueryString.Value);
                    case "ContentType": return new StringValue(request.ContentType);
                    case "ContentLength": return NumberValue.Create(request.ContentLength ?? 0);
                    case "Cookies": return new ObjectValue(new CookieCollectionWrapper(request.Cookies));
                    case "Headers": return new ObjectValue(new HeaderDictionaryWrapper(request.Headers));
                    case "Query": return new ObjectValue(request.Query);
                    case "Form": return request.HasFormContentType ? (FluidValue)new ObjectValue(request.Form) : NilValue.Instance;
                    case "Protocol": return new StringValue(request.Protocol);
                    case "Path": return new StringValue(request.Path.Value);
                    case "PathBase": return new StringValue(request.PathBase.Value);
                    case "Host": return new StringValue(request.Host.Value);
                    case "IsHttps": return BooleanValue.Create(request.IsHttps);
                    case "Scheme": return new StringValue(request.Scheme);
                    case "Method": return new StringValue(request.Method);

                    default: return null;
                }
            });

            TemplateContext.GlobalMemberAccessStrategy.Register<FormCollection, FluidValue>((forms, name) =>
            {
                if (name == "Keys")
                {
                    return new ArrayValue(forms.Keys.Select(x => new StringValue(x)));
                }

                return new ArrayValue(forms[name].Select(x => new StringValue(x)).ToArray());
            });
            
            TemplateContext.GlobalMemberAccessStrategy.Register<HttpContext, FluidValue>((httpcontext, name) =>
            {
                switch (name)
                {
                    case "Items": return new ObjectValue(new HttpContextItemsWrapper(httpcontext.Items));
                    default: return null;
                }
            });

            TemplateContext.GlobalMemberAccessStrategy.Register<HttpContextItemsWrapper, object>((httpContext, name) => httpContext.Items[name]); 
            TemplateContext.GlobalMemberAccessStrategy.Register<QueryCollection, string[]>((queries, name) => queries[name].ToArray());
            TemplateContext.GlobalMemberAccessStrategy.Register<CookieCollectionWrapper, string>((cookies, name) => cookies.RequestCookieCollection[name]);
            TemplateContext.GlobalMemberAccessStrategy.Register<HeaderDictionaryWrapper, string[]>((headers, name) => headers.HeaderDictionary[name].ToArray());            
        }

        public RequestLiquidTemplateEventHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task RenderingAsync(TemplateContext context)
        {
            // Reuse the value as the service can be resolved by multiple templates
            _httpContext = _httpContext ?? _serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext;

            if (_httpContext != null)
            {
                context.SetValue("Request", _httpContext.Request);
                context.SetValue("HttpContext", _httpContext);
            }

            return Task.CompletedTask;
        }

        private class CookieCollectionWrapper
        {
            public readonly IRequestCookieCollection RequestCookieCollection;

            public CookieCollectionWrapper(IRequestCookieCollection requestCookieCollection)
            {
                RequestCookieCollection = requestCookieCollection;
            }
        }

        private class HeaderDictionaryWrapper
        {
            public readonly IHeaderDictionary HeaderDictionary;

            public HeaderDictionaryWrapper(IHeaderDictionary headerDictionary)
            {
                HeaderDictionary = headerDictionary;
            }
        }

        private class HttpContextItemsWrapper
        {
            public readonly IDictionary<object,object> Items;

            public HttpContextItemsWrapper(IDictionary<object,object> items)
            {
                Items = items;
            }
        }
    }
}
