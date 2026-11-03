using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Data;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Recipes.Models;
using OrchardCore.RemoteManagement;
using OrchardCore.Setup.Services;
using OrchardCore.Tenants.Controllers;
using YesSql;

namespace OrchardCore.Tenants.Endpoints.Management;

internal static partial class TenantManagementEndpoints
{
    /// <summary>Records why the setups run from this tenant failed, for the setup status route.</summary>
    public static IServiceCollection AddTenantSetupDiagnostics(this IServiceCollection services)
    {
        services.AddScoped<SetupService>();
        services.AddScoped<ITenantSetupFailureStore, TenantSetupFailureStore>();
        services.AddScoped<ISetupService, FailureRecordingSetupService>(serviceProvider => new FailureRecordingSetupService(
            serviceProvider.GetRequiredService<SetupService>(),
            serviceProvider.GetRequiredService<ITenantSetupFailureStore>(),
            serviceProvider.GetRequiredService<IStringLocalizer<FailureRecordingSetupService>>()));

        return services;
    }

    public static IEndpointRouteBuilder AddTenantSetupDiagnosticsEndpoints(this IEndpointRouteBuilder builder)
    {
        var routes = builder.MapManagementGroup(TenantManagementApiEndpointConventions.TagName);

        routes.MapGet(RoutePrefix + "/setup-options", GetSetupOptionsAsync)
            .WithName("ApiGetTenantSetupOptions")
            .WithSummary("Lists the setup recipes and database providers.")
            .WithDescription("Returns the values accepted by the recipeName and databaseProvider properties when installing or setting up a tenant. Recipes tagged 'hidden' are omitted, although they can still be used by name.")
            .WithCliCommand(new CliOperationMetadata(["tenants", "setup-options"], "show")
            {
                Capability = TenantManagementApiEndpointConventions.CapabilityName,
            })
            .Produces<TenantSetupOptionsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        routes.MapGet(RoutePrefix + "/{tenantName}/setup-status", GetSetupStatusAsync)
            .WithName("ApiGetTenantSetupStatus")
            .WithSummary("Gets the setup status of a tenant.")
            .WithDescription("Returns the state of a tenant and, while it's uninitialized, why its last setup failed.")
            .WithCliCommand(new CliOperationMetadata(["tenants"], "setup-status")
            {
                Capability = TenantManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("tenantName", 0) },
            })
            .Produces<TenantSetupStatusResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    internal static async Task<IResult> GetSetupOptionsAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] ShellSettings currentShellSettings,
        [FromServices] ISetupService setupService,
        [FromServices] IEnumerable<DatabaseProvider> databaseProviders,
        [FromServices] IStringLocalizer<TenantApiController> localizer)
    {
        if (await AuthorizeManageTenantsAsync(httpContext, authorizationService, currentShellSettings, localizer) is { } authError)
        {
            return authError;
        }

        var recipes = await setupService.GetSetupRecipesAsync();

        return TypedResults.Ok(new TenantSetupOptionsResponse
        {
            Recipes = recipes
                .Where(recipe => recipe.Tags is null || !recipe.Tags.Contains("hidden", StringComparer.OrdinalIgnoreCase))
                .OrderBy(recipe => recipe.DisplayName ?? recipe.Name, StringComparer.OrdinalIgnoreCase)
                .Select(recipe => new TenantSetupRecipeResponse
                {
                    Name = recipe.Name,
                    DisplayName = recipe.DisplayName,
                    Description = recipe.Description,
                    Categories = recipe.Categories ?? [],
                    Tags = recipe.Tags ?? [],
                })
                .ToArray(),
            DatabaseProviders = databaseProviders
                .OrderBy(provider => provider.Name, StringComparer.OrdinalIgnoreCase)
                .Select(provider => new TenantDatabaseProviderResponse
                {
                    Value = provider.Value,
                    Name = provider.Name,
                    HasConnectionString = provider.HasConnectionString,
                    HasTablePrefix = provider.HasTablePrefix,
                    IsDefault = provider.IsDefault,
                    SampleConnectionString = provider.SampleConnectionString,
                })
                .ToArray(),
        });
    }

    internal static async Task<IResult> GetSetupStatusAsync(
        HttpContext httpContext,
        string tenantName,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] ShellSettings currentShellSettings,
        [FromServices] IShellHost shellHost,
        [FromServices] ITenantSetupFailureStore failures,
        [FromServices] IStringLocalizer<TenantApiController> localizer)
    {
        if (await AuthorizeManageTenantsAsync(httpContext, authorizationService, currentShellSettings, localizer) is { } authError)
        {
            return authError;
        }

        if (!shellHost.TryGetSettings(tenantName, out var settings))
        {
            return httpContext.ApiNotFoundProblem();
        }

        return TypedResults.Ok(new TenantSetupStatusResponse
        {
            TenantName = settings.Name,
            State = settings.State.ToString(),
            LastFailure = settings.IsUninitialized() ? await failures.FindAsync(settings.Name) : null,
        });
    }

    internal sealed class TenantSetupOptionsResponse
    {
        public TenantSetupRecipeResponse[] Recipes { get; init; } = [];
        public TenantDatabaseProviderResponse[] DatabaseProviders { get; init; } = [];
    }

    internal sealed class TenantSetupRecipeResponse
    {
        public string Name { get; init; }
        public string DisplayName { get; init; }
        public string Description { get; init; }
        public string[] Categories { get; init; } = [];
        public string[] Tags { get; init; } = [];
    }

    internal sealed class TenantDatabaseProviderResponse
    {
        public string Value { get; init; }
        public string Name { get; init; }
        public bool HasConnectionString { get; init; }
        public bool HasTablePrefix { get; init; }
        public bool IsDefault { get; init; }
        public string SampleConnectionString { get; init; }
    }

    internal sealed class TenantSetupStatusResponse
    {
        public string TenantName { get; init; }
        public string State { get; init; }
        public TenantSetupFailure LastFailure { get; init; }
    }
}

/// <summary>Why the last setup of a tenant failed.</summary>
internal sealed class TenantSetupFailure
{
    public string Message { get; init; }
    public DateTime OccurredUtc { get; init; }
}

/// <summary>The failed setups of the tenants, in the database of the tenant managing them as the failed tenants may have none.</summary>
internal sealed class TenantSetupFailureLog
{
    public Dictionary<string, TenantSetupFailure> Failures { get; init; } = [];
}

internal interface ITenantSetupFailureStore
{
    Task<TenantSetupFailure> FindAsync(string tenantName);

    Task RecordAsync(string tenantName, string message);

    Task ClearAsync(string tenantName);
}

internal sealed class TenantSetupFailureStore : ITenantSetupFailureStore
{
    private const int MaxAttempts = 3;

    private readonly IStore _store;
    private readonly IShellHost _shellHost;
    private readonly IClock _clock;
    private readonly ILogger _logger;

    public TenantSetupFailureStore(IStore store, IShellHost shellHost, IClock clock, ILogger<TenantSetupFailureStore> logger)
    {
        _store = store;
        _shellHost = shellHost;
        _clock = clock;
        _logger = logger;
    }

    public async Task<TenantSetupFailure> FindAsync(string tenantName)
    {
        if (_store is null)
        {
            return null;
        }

        using var session = _store.CreateSession();
        var log = await session.Query<TenantSetupFailureLog>().FirstOrDefaultAsync();

        return log?.Failures.GetValueOrDefault(tenantName);
    }

    public Task RecordAsync(string tenantName, string message)
        => UpdateAsync(tenantName, log =>
        {
            // Nothing else removes the failure of a tenant that was deleted while uninitialized.
            foreach (var name in log.Failures.Keys.Where(name => !_shellHost.TryGetSettings(name, out _)).ToArray())
            {
                log.Failures.Remove(name);
            }

            log.Failures[tenantName] = new TenantSetupFailure { Message = message, OccurredUtc = _clock.UtcNow };

            return true;
        });

    public Task ClearAsync(string tenantName)
        => UpdateAsync(tenantName, log => log.Failures.Remove(tenantName));

    private async Task UpdateAsync(string tenantName, Func<TenantSetupFailureLog, bool> update)
    {
        // A Default tenant that isn't set up, which can run this module as a setup feature, has no database.
        if (_store is null)
        {
            return;
        }

        try
        {
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    using var session = _store.CreateSession();
                    var log = await session.Query<TenantSetupFailureLog>().FirstOrDefaultAsync() ?? new TenantSetupFailureLog();

                    if (update(log))
                    {
                        await session.SaveAsync(log, checkConcurrency: true);
                        await session.SaveChangesAsync();
                    }

                    return;
                }
                catch (ConcurrencyException) when (attempt < MaxAttempts)
                {
                    // Another setup changed the log, start over with its new version.
                }
            }
        }
        catch (Exception exception) when (!exception.IsFatal())
        {
            // This is only a diagnostic, it must not change the outcome of the setup.
            _logger.LogError(exception, "Unable to record the outcome of the setup of the tenant '{TenantName}'.", tenantName);
        }
    }
}

/// <summary>Records the outcome of the setups started from this tenant, whichever API, screen or workflow started them.</summary>
internal sealed class FailureRecordingSetupService : ISetupService
{
    private readonly ISetupService _setupService;
    private readonly ITenantSetupFailureStore _failures;
    private readonly IStringLocalizer S;

    public FailureRecordingSetupService(ISetupService setupService, ITenantSetupFailureStore failures, IStringLocalizer<FailureRecordingSetupService> stringLocalizer)
    {
        _setupService = setupService;
        _failures = failures;
        S = stringLocalizer;
    }

    public Task<IEnumerable<RecipeDescriptor>> GetSetupRecipesAsync()
        => _setupService.GetSetupRecipesAsync();

    public async Task<string> SetupAsync(SetupContext context)
    {
        string executionId;

        try
        {
            executionId = await _setupService.SetupAsync(context);
        }
        catch (Exception exception) when (!exception.IsFatal())
        {
            // Exception messages can hold connection strings, so they stay in the logs.
            await _failures.RecordAsync(context.ShellSettings.Name, S["An error occurred while running the setup. Consult the application logs for details."]);
            throw;
        }

        if (context.Errors.Count > 0)
        {
            await _failures.RecordAsync(context.ShellSettings.Name, string.Join(' ', context.Errors.Values));
        }
        else
        {
            await _failures.ClearAsync(context.ShellSettings.Name);
        }

        return executionId;
    }
}
