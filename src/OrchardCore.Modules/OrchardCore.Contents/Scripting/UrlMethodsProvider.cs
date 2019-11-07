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
                     string ret;

                     var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();

                     if (!string.IsNullOrWhiteSpace(shellSettings.RequestUrlPrefix))
                         ret = shellSettings.RequestUrlPrefix.Trim('/');
                     else
                         ret = string.Empty;

                     if (!string.IsNullOrWhiteSpace(path))
                     {
                         ret = string.Concat(ret, '/', path.Trim('/')).Trim('/');
                     }

                     return string.Concat('/', ret);
                 })
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { _getUrlPrefix };
        }
    }
}
