using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.AutoSetup.Extensions;
using OrchardCore.AutoSetup.Options;
using OrchardCore.AutoSetup.Services;
using OrchardCore.Environment.Shell;
using OrchardCore.Locking.Distributed;

namespace OrchardCore.AutoSetup;

/// <summary>
/// The auto setup middleware.
/// </summary>
public class AutoSetupMiddleware
{
    /// <summary>
    /// The next middleware in the execution pipeline.
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// The shell host.
    /// </summary>
    private readonly IShellHost _shellHost;

    /// <summary>
    /// The shell settings.
    /// </summary>
    private readonly ShellSettings _shellSettings;

    /// <summary>
    /// The shell settings manager.
    /// </summary>
    private readonly IShellSettingsManager _shellSettingsManager;

    /// <summary>
    /// A distributed lock guaranties an atomic setup in multi instances environment.
    /// </summary>
    private readonly IDistributedLock _distributedLock;

    /// <summary>
    /// The auto setup options.
    /// </summary>
    private readonly AutoSetupOptions _options;

    /// <summary>
    /// The auto setup lock options.
    /// </summary>
    private readonly LockOptions _lockOptions;

    /// <summary>
    /// The tenant setup options.
    /// </summary>
    private readonly TenantSetupOptions _setupOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoSetupMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the execution pipeline.</param>
    /// <param name="shellHost">The shell host.</param>
    /// <param name="shellSettings">The shell settings.</param>
    /// <param name="shellSettingsManager">The shell settings manager.</param>
    /// <param name="distributedLock">The distributed lock.</param>
    /// <param name="options">The auto setup options.</param>    
    public AutoSetupMiddleware(
        RequestDelegate next,
        IShellHost shellHost,
        ShellSettings shellSettings,
        IShellSettingsManager shellSettingsManager,
        IDistributedLock distributedLock,
        IOptions<AutoSetupOptions> options)
    {
        _next = next;
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _shellSettingsManager = shellSettingsManager;
        _distributedLock = distributedLock;
        _options = options.Value;
        _lockOptions = _options.LockOptions;
        _setupOptions = _options.Tenants.FirstOrDefault(options => _shellSettings.Name == options.ShellName);
    }

    /// <summary>
    /// The auto setup middleware invoke.
    /// </summary>
    /// <param name="httpContext">
    /// The http context.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/>.
    /// </returns>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (_setupOptions is not null && _shellSettings.IsUninitialized())
        {
            // Try to acquire a lock before starting installation, it guaranties an atomic setup in multi instances environment.
            (var locker, var locked) = await _distributedLock.TryAcquireAutoSetupLockAsync(_lockOptions);
            if (!locked)
            {
                throw new TimeoutException($"Fails to acquire an auto setup lock for the tenant: {_setupOptions.ShellName}");
            }

            await using var acquiredLock = locker;

            if (_shellSettings.IsUninitialized())
            {
                var pathBase = httpContext.Request.PathBase;
                if (!pathBase.HasValue)
                {
                    pathBase = "/";
                }

                // Check if the tenant was installed by another instance.
                using var settings = await _shellSettingsManager.LoadSettingsAsync(_shellSettings.Name);

                if (settings != null)
                {
                    settings.AsDisposable();
                    if (!settings.IsUninitialized())
                    {
                        await _shellHost.ReloadShellContextAsync(_shellSettings, eventSource: false);
                        httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                        await httpContext.Response.WriteAsync("The requested tenant is not initialized.");
                        return;
                    }
                }

                var autoSetupService = httpContext.RequestServices.GetRequiredService<IAutoSetupService>();
                (var setupContext, var isSuccess) = await autoSetupService.SetupTenantAsync(_setupOptions, _shellSettings);
                if (isSuccess)
                {
                    if (_setupOptions.IsDefault)
                    {
                        // Create the rest of the shells for further on demand setup.
                        foreach (var setupOptions in _options.Tenants)
                        {
                            if (_setupOptions != setupOptions)
                            {
                                await autoSetupService.CreateTenantSettingsAsync(setupOptions);
                            }
                        }
                    }

                    httpContext.Response.Redirect(pathBase);
                }
                else
                {
                    httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    var stringBuilder = new StringBuilder();
                    foreach (var error in setupContext.Errors)
                    {
                        stringBuilder.AppendLine($"{error.Key} : '{error.Value}'");
                    }

                    await httpContext.Response.WriteAsync($"The AutoSetup failed installing the site '{_setupOptions.SiteName}' with errors: {stringBuilder}.");
                    return;
                }
            }
        }

        await _next.Invoke(httpContext);
    }
}
