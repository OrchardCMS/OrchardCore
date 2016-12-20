using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Scripting;

namespace Orchard.ContentManagement
{
    public class IdGeneratorMethod : IGlobalMethodProvider
    {
        private static GlobalMethod Uuid = new GlobalMethod
        {
            Name = "uuid",
            Method = serviceProvider => (Func<object>) (() =>
            {
                var idGenerator = serviceProvider.GetRequiredService<IContentItemIdGenerator>();
                return idGenerator.GenerateUniqueId(new ContentItem());
            })
        };

        public IEnumerable<GlobalMethod> GetMethods()
        {
            yield return Uuid;
        }
    }
}
