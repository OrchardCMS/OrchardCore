using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public sealed class TwoFactorEmailTokenProvider : DataProtectorTokenProvider<IUser>
{
    public TwoFactorEmailTokenProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<TwoFactorEmailTokenProviderOptions> options,
        ILogger<TwoFactorEmailTokenProvider> logger)
        : base(dataProtectionProvider, options, logger)
    {
    }

    public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<IUser> manager, IUser user)
        => Task.FromResult(true);
}
