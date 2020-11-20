using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Scripting;

namespace OrchardCore.Entities.Scripting
{
    public class AdminOptionsMethod : IGlobalMethodProvider
    {
        private static readonly GlobalMethod AdminOptions = new GlobalMethod
        {
            Name = "adminurlprefix",
            Method = serviceProvider => (Func<string>)(() =>
           {
               var adminOptions = serviceProvider.GetRequiredService<IOptions<AdminOptions>>();
               return adminOptions.Value.AdminUrlPrefix;
           })
        };

        public IEnumerable<GlobalMethod> GetMethods()
        {
            yield return AdminOptions;
        }
    }
}
