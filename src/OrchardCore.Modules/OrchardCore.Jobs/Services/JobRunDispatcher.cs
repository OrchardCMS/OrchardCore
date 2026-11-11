using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.Jobs.Services;

/// <summary>
/// Executes queued runs in the background, each in its own scope, detached from the request or background task that
/// queued it, so a long run neither delays a response nor blocks the background tasks. At most
/// <see cref="JobExecutionOptions.MaxConcurrentRuns"/> runs of the tenant execute at once on this server; the others
/// wait for one to end, or are taken by another server. Registered as a tenant singleton.
/// </summary>
public sealed class JobRunDispatcher
{
    // Above this delay a run is left to the background task, which starts it when it is due.
    private static readonly TimeSpan _maxDelay = TimeSpan.FromMinutes(2);

    // Limits the runs of all the tenants of this server, when configured: a tenant singleton can't hold it.
    private static SemaphoreSlim _serverSlots;
    private static readonly Lock _serverSlotsLock = new();

    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly JobRunTracker _tracker;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IClock _clock;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _slots;
    private readonly int _maxConcurrentRunsPerServer;
    private readonly ConcurrentDictionary<string, bool> _dispatched = new(StringComparer.Ordinal);

    public JobRunDispatcher(
        IShellHost shellHost,
        ShellSettings shellSettings,
        JobRunTracker tracker,
        IHttpContextAccessor httpContextAccessor,
        IClock clock,
        IOptions<JobExecutionOptions> options,
        ILogger<JobRunDispatcher> logger)
    {
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _tracker = tracker;
        _httpContextAccessor = httpContextAccessor;
        _clock = clock;
        _logger = logger;

        var slots = Math.Max(1, options.Value.MaxConcurrentRuns);
        _slots = new SemaphoreSlim(slots, slots);
        _maxConcurrentRunsPerServer = options.Value.MaxConcurrentRunsPerServer;
    }

    /// <summary>
    /// Gets the number of runs that this server holds: the ones it executes, and the ones that wait for their due
    /// date or for a free slot.
    /// </summary>
    public int PendingCount => _dispatched.Count;

    /// <summary>
    /// Starts a queued run in the background once it is due and fewer than
    /// <see cref="JobExecutionOptions.MaxConcurrentRuns"/> runs execute, unless it already executes or waits on
    /// this server. It does nothing when another server took the run in between.
    /// </summary>
    /// <param name="runId">The run.</param>
    /// <param name="dueUtc">When the run is due, or <see langword="null"/> when it is due now.</param>
    public void Dispatch(string runId, DateTime? dueUtc = null)
    {
        if (string.IsNullOrEmpty(runId) || _tracker.Stopping.IsCancellationRequested)
        {
            return;
        }

        var delay = dueUtc is null ? TimeSpan.Zero : dueUtc.Value - _clock.UtcNow;

        if (delay > _maxDelay || _tracker.IsRunning(runId) || !_dispatched.TryAdd(runId, true))
        {
            return;
        }

        // Don't let the run inherit the shell scope and the HTTP context of the caller, which end before the run.
        using (ExecutionContext.SuppressFlow())
        {
            _ = Task.Run(() => ExecuteAsync(runId, delay));
        }
    }

    private async Task ExecuteAsync(string runId, TimeSpan delay)
    {
        var acquired = false;
        SemaphoreSlim serverSlots = null;
        DateTime? retryUtc = null;

        try
        {
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, _tracker.Stopping);
            }

            await _slots.WaitAsync(_tracker.Stopping);
            acquired = true;

            var slots = GetServerSlots();

            if (slots is not null)
            {
                await slots.WaitAsync(_tracker.Stopping);
                serverSlots = slots;
            }

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
                    retryUtc = await scope.ServiceProvider.GetRequiredService<JobRunExecutor>().ExecuteAsync(runId);
                }
                finally
                {
                    _httpContextAccessor.HttpContext = null;
                }
            });
        }
        catch (OperationCanceledException) when (_tracker.Stopping.IsCancellationRequested)
        {
            // The tenant is released; the run stays queued for another server, or for the next time it starts.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The job run '{RunId}' of the tenant '{TenantName}' couldn't be executed.", runId, _shellSettings.Name);
        }
        finally
        {
            serverSlots?.Release();

            if (acquired)
            {
                _slots.Release();
            }

            _dispatched.TryRemove(runId, out _);
        }

        if (retryUtc is not null)
        {
            // The next attempt of the run, on this server unless another one takes it first.
            Dispatch(runId, retryUtc);
        }
    }

    private SemaphoreSlim GetServerSlots()
    {
        if (_maxConcurrentRunsPerServer <= 0)
        {
            return null;
        }

        if (_serverSlots is null)
        {
            lock (_serverSlotsLock)
            {
                _serverSlots ??= new SemaphoreSlim(_maxConcurrentRunsPerServer, _maxConcurrentRunsPerServer);
            }
        }

        return _serverSlots;
    }

    private static async Task SetBaseUrlAsync(ShellScope scope, HttpContext httpContext)
    {
        // Links a run creates, such as the links of an email, must point to the site, as when they come from a request.
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
