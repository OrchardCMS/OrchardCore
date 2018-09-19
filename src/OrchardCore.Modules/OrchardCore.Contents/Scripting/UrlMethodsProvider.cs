using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
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
                Method = serviceProvider => (Func<string, string>)((string path) =>
                 {
                     var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
                     var url = string.Concat('/', shellSettings.RequestUrlPrefix).TrimEnd('/');
                     if (!string.IsNullOrWhiteSpace(path))
                     {
                         url = string.Concat(url, '/', path.TrimStart('/')).TrimEnd('/');
                     }
                     return url;
                 })
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { _getUrlPrefix };
        }
    }
}
