using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OrchardCore.BackgroundTasks;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Services;

namespace OrchardCore.OpenId.Tasks
{
    [BackgroundTask(Schedule = "*/30 * * * *", Description = "Performs various cleanup operations for OpenID-related features.")]
    public class OpenIdBackgroundTask : IBackgroundTask
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
                await _serverService.PruneSigningKeysAsync();
            }
            catch (OperationCanceledException exception) when (exception.CancellationToken == cancellationToken)
            {
                _logger.LogDebug("The X.509 certificates pruning task was aborted.");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred while pruning X.509 certificates from the shell storage.");
            }

            // Note: this background task is also responsible of automatically removing orphaned tokens/authorizations
            // (i.e tokens that are no longer valid and ad-hoc authorizations that have no valid tokens associated).
            // Since ad-hoc authorizations and their associated tokens are removed as part of the same operation
            // when they no longer have any token attached, it's more efficient to remove the authorizations first.

            // Note: the authorization/token managers MUST be resolved from the scoped provider
            // as they depend on scoped stores that should be disposed as soon as possible.

            try
            {
                await serviceProvider.GetRequiredService<IOpenIdAuthorizationManager>().PruneAsync(cancellationToken);
            }
            catch (OpenIddictExceptions.ConcurrencyException exception)
            {
                _logger.LogDebug(exception, "A concurrency error occurred while pruning authorizations from the database.");
            }
            catch (OperationCanceledException exception) when (exception.CancellationToken == cancellationToken)
            {
                _logger.LogDebug("The authorizations pruning task was aborted.");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred while pruning authorizations from the database.");
            }

            try
            {
                await serviceProvider.GetRequiredService<IOpenIdTokenManager>().PruneAsync(cancellationToken);
            }
            catch (OpenIddictExceptions.ConcurrencyException exception)
            {
                _logger.LogDebug(exception, "A concurrency error occurred while pruning tokens from the database.");
            }
            catch (OperationCanceledException exception) when (exception.CancellationToken == cancellationToken)
            {
                _logger.LogDebug("The tokens pruning task was aborted.");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred while pruning tokens from the database.");
            }
        }
    }
}
