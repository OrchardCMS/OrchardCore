using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Settings;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Executes queued runs in the background, each in its own scope, detached from the request or background task that
/// queued it, so a long run neither delays a response nor blocks the other background tasks. At most
/// <see cref="DataPipelineOptions.MaxConcurrentRuns"/> runs of the tenant execute at once; the others stay queued until
/// one ends. Registered as a tenant singleton.
/// </summary>
public sealed class DataPipelineRunDispatcher
{
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly DataPipelineRunTracker _tracker;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _slots;
    private readonly ConcurrentDictionary<string, bool> _dispatched = new(StringComparer.Ordinal);

    public DataPipelineRunDispatcher(
        IShellHost shellHost,
        ShellSettings shellSettings,
        DataPipelineRunTracker tracker,
        IHttpContextAccessor httpContextAccessor,
        IOptions<DataPipelineOptions> options,
        ILogger<DataPipelineRunDispatcher> logger)
    {
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _tracker = tracker;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;

        var slots = Math.Max(1, options.Value.MaxConcurrentRuns);
        _slots = new SemaphoreSlim(slots, slots);
    }

    /// <summary>
    /// Starts a queued run in the background as soon as fewer than <see cref="DataPipelineOptions.MaxConcurrentRuns"/>
    /// runs execute, unless it already executes or waits on this server.
    /// </summary>
    /// <param name="runId">The run.</param>
    public void Dispatch(string runId)
    {
        if (string.IsNullOrEmpty(runId) || _tracker.IsRunning(runId) || _tracker.Stopping.IsCancellationRequested || !_dispatched.TryAdd(runId, true))
        {
            return;
        }

        // Don't let the run inherit the shell scope and the HTTP context of the caller, which end before the run.
        using (ExecutionContext.SuppressFlow())
        {
            _ = Task.Run(() => ExecuteAsync(runId));
        }
    }

    private async Task ExecuteAsync(string runId)
    {
        var acquired = false;

        try
        {
            await _slots.WaitAsync(_tracker.Stopping);
            acquired = true;

            if (!_shellSettings.IsRunning())
            {
                return;
            }

            var shellScope = await _shellHost.GetScopeAsync(_shellSettings);

            await shellScope.UsingAsync(async scope =>
            {
                var httpContext = scope.ShellContext.CreateHttpContext();
                await SetBaseUrlAsync(scope, httpContext);
                _httpContextAccessor.HttpContext = httpContext;

                try
                {
                    await scope.ServiceProvider.GetRequiredService<DataPipelineRunExecutor>().ExecuteAsync(runId);
                }
                finally
                {
                    _httpContextAccessor.HttpContext = null;
                }
            });
        }
        catch (OperationCanceledException) when (_tracker.Stopping.IsCancellationRequested)
        {
            // The tenant is released; the run stays queued for the next time it starts.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The data pipeline run '{RunId}' of the tenant '{TenantName}' couldn't be executed.", runId, _shellSettings.Name);
        }
        finally
        {
            if (acquired)
            {
                _slots.Release();
            }

            _dispatched.TryRemove(runId, out _);
        }
    }

    private static async Task SetBaseUrlAsync(ShellScope scope, HttpContext httpContext)
    {
        // Links the run sends, such as download links, must point to the site, as when the run comes from a request.
        var siteService = scope.ServiceProvider.GetService<ISiteService>();
        var baseUrl = siteService is null ? null : (await siteService.GetSiteSettingsAsync()).BaseUrl;

        if (string.IsNullOrEmpty(baseUrl) || !Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
        {
            return;
        }

        httpContext.Request.Scheme = uri.Scheme;
        httpContext.Request.Host = new HostString(uri.Host, uri.Port);

        if (uri.AbsolutePath.Length > 1)
        {
            httpContext.Request.PathBase = uri.AbsolutePath.TrimEnd('/');
        }
    }
}
