using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Json;
using OrchardCore.Deployment.Artifacts;
using OrchardCore.Deployment.Core.Services;
using OrchardCore.Deployment.Services;
using OrchardCore.Deployment.Steps;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.Deployment.Operations;

internal interface IDeploymentOperationExecutor
{
    Task<string> ExecuteAsync(DeploymentOperation operation, CancellationToken cancellationToken);
}

internal sealed class DeploymentOperationExecutor : IDeploymentOperationExecutor
{
    public async Task<string> ExecuteAsync(DeploymentOperation operation, CancellationToken cancellationToken)
    {
        string artifactId = null;
        await ShellScope.UsingChildScopeAsync(async scope =>
        {
            var services = scope.ServiceProvider;
            var artifacts = services.GetRequiredService<DeploymentArtifactStore>();
            var principal = await AuthorizeCallerAsync(services.GetRequiredService<IAuthorizationService>(), operation);
            services.GetRequiredService<DeploymentExecutionContext>().User = principal;
            if (operation.Kind == DeploymentOperationKind.Export)
            {
                var options = services.GetRequiredService<IOptions<DocumentJsonSerializerOptions>>().Value.SerializerOptions;
                var plan = JsonSerializer.Deserialize<DeploymentPlan>(operation.Payload, options);
                if (plan is null || plan.DeploymentSteps.Any(step => step is UnknownDeploymentStep))
                {
                    throw new InvalidOperationException("The export plan contains unavailable deployment steps.");
                }
                await using var archive = await services.GetRequiredService<IDeploymentArchiveService>()
                    .CreateAsync(plan, DeploymentRecipeMetadata.Create(plan));
                var artifact = await artifacts.CreateAsync(operation.Owner, DeploymentArtifactKind.Export,
                    plan.Name.ToSafeName() + ".zip", "application/zip", archive, cancellationToken);
                artifactId = artifact.Id;
            }
            else
            {
                using var lease = await artifacts.OpenAsync(operation.Payload, operation.Owner);
                if (lease is null || lease.Artifact.Kind != DeploymentArtifactKind.Import)
                {
                    throw new InvalidOperationException("The import artifact is unavailable.");
                }
                using var package = await services.GetRequiredService<DeploymentPackageService>()
                    .StageAsync(lease.Stream, lease.Artifact.FileName, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                // Recipe steps resolve the current user from the HTTP context, e.g. to set the owner of a content item.
                var httpContext = services.GetRequiredService<IHttpContextAccessor>().HttpContext;
                var previousUser = httpContext?.User;
                if (httpContext is not null) { httpContext.User = principal; }
                try
                {
                    await services.GetRequiredService<IDeploymentManager>().ImportDeploymentPackageAsync(package.FileProvider);
                }
                finally
                {
                    if (httpContext is not null) { httpContext.User = previousUser; }
                }
            }
        });
        return artifactId;
    }

    // The permissions of the caller may have changed since the operation was accepted.
    internal static async Task<ClaimsPrincipal> AuthorizeCallerAsync(IAuthorizationService authorization, DeploymentOperation operation)
    {
        var principal = operation.Identity?.Restore();
        var permission = operation.Kind == DeploymentOperationKind.Export ? DeploymentPermissions.Export : DeploymentPermissions.Import;
        if (principal is null || DeploymentArtifactOwner.Get(principal) != operation.Owner
            || !await authorization.AuthorizeAsync(principal, permission))
        {
            throw new UnauthorizedAccessException("The operation has no authorized initiating identity.");
        }
        return principal;
    }
}

internal sealed class DeploymentOperationRunner
{
    private readonly DeploymentOperationStore _store;
    private readonly IDeploymentOperationExecutor _executor;
    private readonly ILogger<DeploymentOperationRunner> _logger;

    public DeploymentOperationRunner(DeploymentOperationStore store, IDeploymentOperationExecutor executor, ILogger<DeploymentOperationRunner> logger)
    {
        _store = store; _executor = executor; _logger = logger;
    }
    public async Task RunAsync(string id, CancellationToken cancellationToken)
    {
        using var claim = await _store.ClaimAsync(id, cancellationToken);
        if (claim is null) { return; }
        string artifactId;
        try
        {
            artifactId = await _executor.ExecuteAsync(claim.Operation, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Keep Running when shutdown prevents a durable update. A future worker
            // observes the abandoned claim and marks it Uncertain without replaying it.
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Deployment operation {OperationId} failed.", id);
            await claim.CompleteAsync(DeploymentOperationState.Failed, null, "execution_failed", cancellationToken);
            return;
        }
        await claim.CompleteAsync(DeploymentOperationState.Succeeded, artifactId, null, cancellationToken);
    }
}

/// <summary>Finds durable deployment work and executes it in the current tenant.</summary>
[BackgroundTask(Schedule = "* * * * *", Description = "Execute queued deployment exports and imports.")]
public sealed class DeploymentOperationTask : IBackgroundTask
{
    /// <summary>Processes pending operations and recovers abandoned execution records without replay.</summary>
    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var store = serviceProvider.GetRequiredService<DeploymentOperationStore>();
        var runner = serviceProvider.GetRequiredService<DeploymentOperationRunner>();
        foreach (var id in await store.PendingAsync(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await runner.RunAsync(id, cancellationToken);
        }
    }
}
