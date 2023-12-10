using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Scripting;

namespace OrchardCore.Secrets.Scripting;

public class DecryptMethodProvider : IGlobalMethodProvider
{
    private readonly GlobalMethod _globalMethod;

    public DecryptMethodProvider()
    {
        _globalMethod = new GlobalMethod
        {
            Name = "decrypt",
            Method = serviceProvider => (Func<string, object>)(protectedData =>
            {
                var decryptor = serviceProvider
                    .GetRequiredService<ISecretProtectionProvider>()
                    .CreateDecryptorAsync(protectedData).GetAwaiter().GetResult();

                return decryptor.Decrypt();
            })
        };
    }

    public IEnumerable<GlobalMethod> GetMethods()
    {
        yield return _globalMethod;
    }
}
