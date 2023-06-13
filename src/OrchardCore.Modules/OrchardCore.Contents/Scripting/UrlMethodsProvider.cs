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
                Method = serviceProvider => (string path) =>
                {
                    var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
                    return String.Concat('/', $"{shellSettings.RequestUrlPrefix}/{path?.Trim('/')}".Trim('/'));
                }
            };
        }

        public IEnumerable<GlobalMethod> GetMethods() => new[] { _getUrlPrefix };
    }
}
