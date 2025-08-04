using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Users.Handlers;

public class UserDisabledEventHandler : UserEventHandlerBase, IDisposable
{
    private readonly IServiceProvider _serviceProvider;

    private UserManager<IUser> _userManager;

    public UserDisabledEventHandler(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public override async Task DisabledAsync(UserContext context)
    {
        _userManager ??= _serviceProvider.GetRequiredService<UserManager<IUser>>();

        await _userManager.UpdateSecurityStampAsync(context.User);
    }

    public void Dispose() => _userManager?.Dispose();
}
