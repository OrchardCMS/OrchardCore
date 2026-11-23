using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;
using OrchardCore.OpenId.Services;

namespace OrchardCore.OpenId.Tasks;

/// <summary>
/// Removes the expired X.509 certificates from the storage of the tenant. Every server does it, as each one may have
/// a storage of its own. The tokens and the authorizations, which are in the database, are pruned by the
/// <see cref="OpenIdPruningJob"/>, once whatever the number of servers.
/// </summary>
[BackgroundTask(
    Title = "OpenID Cleaner",
    Schedule = "*/30 * * * *",
    Description = "Removes the expired X.509 certificates of the OpenID features from the storage of the server.")]
public sealed class OpenIdBackgroundTask : IBackgroundTask
{
    private readonly ILogger _logger;
    private readonly IOpenIdServerService _serverService;

    public OpenIdBackgroundTask(
        ILogger<OpenIdBackgroundTask> logger,
        IOpenIdServerService serverService)
    {
        _logger = logger;
        _serverService = serverService;
    }

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        try
        {
            await _serverService.PruneManagedCertificatesAsync();
        }
        catch (OperationCanceledException exception) when (exception.CancellationToken == cancellationToken)
        {
            _logger.LogDebug("The X.509 certificates pruning task was aborted.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred while pruning X.509 certificates from the shell storage.");
        }
    }
}
