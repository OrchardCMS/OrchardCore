using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OrchardCore.BackgroundTasks;
using OrchardCore.OpenId.Abstractions.Managers;

namespace OrchardCore.OpenId.Tasks
{
    [BackgroundTask(Group = "OrchardCore.OpenId")]
    public class OpenIdBackgroundTask : IBackgroundTask
    {
        private readonly ILogger<OpenIdBackgroundTask> _logger;

        public OpenIdBackgroundTask(ILogger<OpenIdBackgroundTask> logger)
        {
            _logger = logger;
        }

        public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            // Note: this background task is responsible of automatically removing orphaned tokens/authorizations
            // (i.e tokens that are no longer valid and ad-hoc authorizations that have no valid tokens associated).
            // Since ad-hoc authorizations and their associated tokens are removed as part of the same operation
            // when they no longer have any token attached, it's more efficient to remove the authorizations first.

            // Note: the authorization/token managers MUST be resolved from the scoped provider
            // as they depend on scoped stores that should be disposed as soon as possible.

            try
            {
                await serviceProvider.GetRequiredService<IOpenIdAuthorizationManager>().PruneAsync(cancellationToken);
            }
            catch (OpenIddictException exception) when (exception.Reason == OpenIddictConstants.Exceptions.ConcurrencyError)
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
            catch (OpenIddictException exception) when (exception.Reason == OpenIddictConstants.Exceptions.ConcurrencyError)
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
