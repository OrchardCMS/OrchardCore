using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Scripting;

namespace OrchardCore.Contents.Scripting
{
    public class UrlMethodsProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _getUrlPrefix;

        public UrlMethodsProvider()
        {
            _getUrlPrefix = new GlobalMethod
            {
                Name = "getUrlPrefix",
                Method = serviceProvider => (string path, bool? escaped) =>
                {
                    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

                    var pathBase = httpContextAccessor.HttpContext?.Request.PathBase ?? PathString.Empty;
                    if (!pathBase.HasValue)
                    {
                        pathBase = "/";
                    }

                    path = path?.Trim(' ', '/') ?? String.Empty;
                    if (path.Length > 0)
                    {
                        pathBase = pathBase.Add($"/{path}");
                    }

                    if (escaped.HasValue && escaped.Value)
                    {
                        return pathBase.ToString();
                    }

                    return pathBase.Value;
                },
            };
        }

        public IEnumerable<GlobalMethod> GetMethods() => new[] { _getUrlPrefix };
    }
}
