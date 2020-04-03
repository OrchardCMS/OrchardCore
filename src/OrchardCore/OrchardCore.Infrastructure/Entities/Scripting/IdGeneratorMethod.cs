using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Scripting;

namespace OrchardCore.Entities.Scripting
{
    public class IdGeneratorMethod : IGlobalMethodProvider
    {
        private static GlobalMethod Uuid = new GlobalMethod
        {
            Name = "uuid",
            Method = serviceProvider => (Func<object>) (() =>
            {
                var idGenerator = serviceProvider.GetRequiredService<IIdGenerator>();
                return idGenerator.GenerateUniqueId();
            })
        };

        public IEnumerable<GlobalMethod> GetMethods()
        {
            yield return Uuid;
        }
    }
}
