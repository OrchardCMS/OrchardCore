using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Secrets;
using OrchardCore.Scripting;

namespace OrchardCore.Recipes
{
    public class DecryptMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _globalMethod;

        public DecryptMethodProvider() 
        {
            _globalMethod = new GlobalMethod
            {
                Name = "decrypt",
                Method = serviceProvider => (Func<string, string, object>)((aesKey, protectedData) =>
                {
                    var decryptionService = serviceProvider.GetRequiredService<IDecryptionService>();
                    return decryptionService.DecryptAsync(aesKey, protectedData).GetAwaiter().GetResult();
                })
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            yield return _globalMethod;
        }
    }
}
