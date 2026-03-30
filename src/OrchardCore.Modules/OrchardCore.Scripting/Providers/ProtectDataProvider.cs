using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Scripting.Providers;

public sealed class ProtectDataProvider : IGlobalMethodProvider
{
    private static readonly GlobalMethod _protect = new()
    {
        Name = "protect",
        Method = serviceProvider => (Func<string, string, string>)((purpose, value) =>
        {
            ArgumentException.ThrowIfNullOrEmpty(purpose);
            ArgumentException.ThrowIfNullOrEmpty(value);

            var dataProtectionProvider = serviceProvider.GetRequiredService<IDataProtectionProvider>();
            var protector = dataProtectionProvider.CreateProtector(purpose);

            return protector.Protect(value);
        }),
    };

    public IEnumerable<GlobalMethod> GetMethods()
    {
        return [_protect];
    }
}
