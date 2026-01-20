using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Scripting;

namespace OrchardCore.Entities.Scripting
{
    public class IdGeneratorMethod : IGlobalMethodProvider
    {
        private static readonly GlobalMethod _uuid = new()
        {
            Name = "uuid",
            Method = serviceProvider => () =>
           {
               var idGenerator = serviceProvider.GetRequiredService<IIdGenerator>();
               return idGenerator.GenerateUniqueId();
           },
        };

        public IEnumerable<GlobalMethod> GetMethods()
        {
            yield return _uuid;
        }
    }
}
