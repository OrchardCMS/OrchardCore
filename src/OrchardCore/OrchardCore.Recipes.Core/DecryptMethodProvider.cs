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
        private readonly string _encryptionKey;

        public DecryptMethodProvider(string encryptionKey)
        {
            _encryptionKey = encryptionKey;
            _globalMethod = new GlobalMethod
            {
                Name = "decrypt",
                Method = serviceProvider => (Func<string, object>)(protectedData =>
                {
                    var decryptionService = serviceProvider.GetRequiredService<IDecryptionService>();
                    return decryptionService.DecryptAsync(encryptionKey, protectedData).GetAwaiter().GetResult();
                })
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            yield return _globalMethod;
        }
    }
}
