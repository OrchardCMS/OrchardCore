using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;
using OrchardCore.OpenId.Abstractions.Models;
using OrchardCore.OpenId.Services.Managers;

namespace OrchardCore.OpenId.Services
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

            await DeleteAuthorizationsAsync();
            await DeleteTokensAsync();

            async Task DeleteAuthorizationsAsync()
            {
                // Note: the authorization manager MUST be resolved from the scoped provider
                // as it depends on scoped stores that should be disposed as soon as possible.
                var manager = serviceProvider.GetRequiredService<OpenIdAuthorizationManager>();

                ImmutableArray<IOpenIdAuthorization> authorizations;

                do
                {
                    // Note: don't use an offset here, as the elements returned by this method
                    // are progressively removed from the database immediately after calling it.
                    authorizations = await manager.ListInvalidAsync(100, 0, cancellationToken);

                    foreach (var authorization in authorizations)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            await manager.DeleteAsync(authorization, cancellationToken);

                            _logger.LogDebug("The authorization {AuthorizationId} was successfully removed from the database.",
                                await manager.GetIdAsync(authorization, cancellationToken));
                        }

                        catch (Exception exception)
                        {
                            _logger.LogDebug(exception,
                                "An error occurred while removing the authorization {AuthorizationId} from the database.",
                                await manager.GetIdAsync(authorization, cancellationToken));
                        }
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }

                while (!authorizations.IsDefaultOrEmpty);
            }

            async Task DeleteTokensAsync()
            {
                // Note: the token manager MUST be resolved from the scoped provider as it
                // depends on scoped stores that should be disposed as soon as possible.
                var manager = serviceProvider.GetRequiredService<OpenIdTokenManager>();

                ImmutableArray<IOpenIdToken> tokens;

                do
                {
                    // Note: don't use an offset here, as the elements returned by this method
                    // are progressively removed from the database immediately after calling it.
                    tokens = await manager.ListInvalidAsync(100, 0, cancellationToken);

                    foreach (var token in tokens)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            await manager.DeleteAsync(token, cancellationToken);

                            _logger.LogDebug("The token {TokenId} was successfully removed from the database.",
                                await manager.GetIdAsync(token, cancellationToken));
                        }

                        catch (Exception exception)
                        {
                            _logger.LogDebug(exception,
                                "An error occurred while removing the token {TokenId} from the database.",
                                await manager.GetIdAsync(token, cancellationToken));
                        }
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }

                while (!tokens.IsDefaultOrEmpty);
            }
        }
    }
}
