using System.Threading;

namespace OrchardCore.Users.Services;

public interface IPasswordAuthenticationTimingService
{
    Task MitigateUnknownUserAsync(string password, CancellationToken cancellationToken = default);

    Task DelayFailedAuthenticationAsync(CancellationToken cancellationToken = default);
}
