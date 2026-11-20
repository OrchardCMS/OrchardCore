using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OrchardCore.Jobs;
using OrchardCore.OpenId.Abstractions.Managers;

namespace OrchardCore.OpenId.Tasks;

/// <summary>
/// Removes from the database the tokens that are no longer valid, and the ad-hoc authorizations that have no valid
/// token anymore. It runs every 30 minutes, once whatever the number of servers.
/// </summary>
public sealed class OpenIdPruningJob : IJob
{
    /// <summary>
    /// The name of the job.
    /// </summary>
    public const string Name = "OpenIdPruning";

    // The managers depend on scoped stores, so they come from the scope of the run.
    private readonly IOpenIdTokenManager _tokenManager;
    private readonly IOpenIdAuthorizationManager _authorizationManager;
    private readonly ILogger _logger;

    public OpenIdPruningJob(
        IOpenIdTokenManager tokenManager,
        IOpenIdAuthorizationManager authorizationManager,
        ILogger<OpenIdPruningJob> logger)
    {
        _tokenManager = tokenManager;
        _authorizationManager = authorizationManager;
        _logger = logger;
    }

    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        // Note: this job uses a hardcoded minimum token/authorization lifespan of 14 days,
        // which matches the default value used by the OpenIddict Quartz.NET integration.
        var threshold = DateTimeOffset.UtcNow - TimeSpan.FromDays(14);

        // Important: since tokens associated to ad-hoc authorizations are not removed as part of the same operation,
        // the tokens MUST be deleted before removing the ad-hoc authorizations that no longer have any token.
        try
        {
            var tokens = await _tokenManager.PruneAsync(threshold, cancellationToken);
            context.LogInformation($"{tokens} tokens were removed.");
        }
        catch (OpenIddictExceptions.ConcurrencyException exception)
        {
            // A token was changed while it was removed: the next run removes it.
            _logger.LogDebug(exception, "A concurrency error occurred while pruning tokens from the database.");
            context.LogWarning("Some tokens were changed while they were removed, and are left to the next run.");
        }

        try
        {
            var authorizations = await _authorizationManager.PruneAsync(threshold, cancellationToken);
            context.LogInformation($"{authorizations} authorizations were removed.");
        }
        catch (OpenIddictExceptions.ConcurrencyException exception)
        {
            _logger.LogDebug(exception, "A concurrency error occurred while pruning authorizations from the database.");
            context.LogWarning("Some authorizations were changed while they were removed, and are left to the next run.");
        }
    }
}
