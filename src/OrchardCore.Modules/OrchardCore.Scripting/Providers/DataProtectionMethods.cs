using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace OrchardCore.Scripting.Providers;

internal sealed class DataProtectionMethods : IGlobalMethodProvider
{
    private static readonly GlobalMethod[] _methods =
    [
        new ()
        {
            Name = "encrypt",
            Method = serviceProvider => (Func<string, string>)(data =>
            {
                var protectionProvider = serviceProvider.GetDataProtectionProvider();
                var protector = protectionProvider.CreateProtector(OrchardCoreConstants.Security.ScriptingEncryptionPurpose);
                var bytes = Encoding.UTF8.GetBytes(data);
                var encryptedBytes = protector.Protect(bytes);
                return Convert.ToBase64String(encryptedBytes);
            }),
        },
        new()
        {
            Name = "decrypt",
            Method = serviceProvider => (Func<string, string>)(data =>
            {
                if (string.IsNullOrEmpty(data))
                {
                    return string.Empty;
                }

                try
                {
                    var encryptedbytes = Convert.FromBase64String(data);

                    var protectionProvider = serviceProvider.GetDataProtectionProvider();
                    var protector = protectionProvider.CreateProtector(OrchardCoreConstants.Security.ScriptingEncryptionPurpose);
                    var bytes = protector.Unprotect(encryptedbytes);
                    return Encoding.UTF8.GetString(bytes);
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }),
        }
    ];

    public IEnumerable<GlobalMethod> GetMethods()
    {
        return _methods;
    }
}
