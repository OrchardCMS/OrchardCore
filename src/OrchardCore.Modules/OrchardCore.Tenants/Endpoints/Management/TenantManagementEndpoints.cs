using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Abstractions.Setup;
using OrchardCore.Data;
using OrchardCore.Email;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Recipes.Models;
using OrchardCore.RemoteManagement;
using OrchardCore.Setup.Services;
using OrchardCore.Tenants.Controllers;
using OrchardCore.Tenants.Models;
using OrchardCore.Tenants.Services;

namespace OrchardCore.Tenants.Endpoints.Management;

internal static partial class TenantManagementEndpoints
{
    private const string RoutePrefix = "api/tenants";

    public static IEndpointRouteBuilder AddTenantManagementEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapManagementGet(RoutePrefix, ListAsync)
            .WithName("ApiListTenants")
            .WithSummary("Lists tenants.")
            .WithDescription("Returns tenants that can be managed from the Default tenant, including exact tenant URLs when they can be derived safely.")
            .WithCliCommand(new CliOperationMetadata(["tenants"], "list")
            {
                Capability = TenantManagementApiEndpointConventions.CapabilityName,
                TableColumns =
                {
                    new CliTableColumnMetadata("items.name", "Name"),
                    new CliTableColumnMetadata("items.state", "State"),
                    new CliTableColumnMetadata("items.category", "Category"),
                    new CliTableColumnMetadata("items.primaryUrl", "Url"),
                },
            })
            .Produces<TenantListResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        builder.MapManagementGet(RoutePrefix + "/{tenantName}", GetAsync)
            .WithName("ApiGetTenant")
            .WithSummary("Gets a tenant.")
            .WithDescription("Returns a single tenant with redacted connection details and exact URLs when they can be derived safely.")
            .WithCliCommand(new CliOperationMetadata(["tenants"], "show")
            {
                Capability = TenantManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("tenantName", 0) },
            })
            .Produces<TenantResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        builder.MapManagementPost(RoutePrefix, CreateAsync)
            .WithName("ApiCreateTenantManagement")
            .WithSummary("Creates a tenant.")
            .WithDescription("Creates a tenant shell settings document and returns the created tenant with any setup URL that can be derived safely.")
            .WithCliCommand(new CliOperationMetadata(["tenants"], "create")
            {
                Capability = TenantManagementApiEndpointConventions.CapabilityName,
            })
            .Accepts<TenantCreateRequest>("application/json")
            .Produces<TenantResponse>(StatusCodes.Status201Created)
            .Produces<TenantResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        builder.MapManagementPost(RoutePrefix + "/{tenantName}:install", InstallAsync)
            .WithName("ApiInstallTenantManagement")
            .WithSummary("Creates and sets up a tenant.")
            .WithDescription("Creates a new tenant, executes its setup recipe, and creates its administrator. Existing names are rejected. If setup fails, the created tenant is preserved for diagnosis and a separate setup retry.")
            .WithCliCommand(new CliOperationMetadata(["tenants"], "install")
            {
                Capability = TenantManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("tenantName", 0) },
                SecretProperties = { "password", "connectionString" },
            })
            .Accepts<TenantInstallRequest>("application/json")
            .Produces<TenantResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem();

        builder.MapManagementPost(RoutePrefix + "/{tenantName}:setup", SetupAsync)
            .WithName("ApiSetupTenantManagement")
            .WithSummary("Sets up a tenant.")
            .WithDescription("Initializes a tenant, creates its administrator account, and executes its setup recipe.")
            .WithCliCommand(new CliOperationMetadata(["tenants"], "setup")
            {
                Capability = TenantManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("tenantName", 0) },
                SecretProperties = { "password", "connectionString" },
            })
            .Accepts<TenantSetupRequest>("application/json")
            .Produces<TenantResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem();

        builder.MapManagementPut(RoutePrefix + "/{tenantName}", UpdateAsync)
            .WithName("ApiUpdateTenantManagement")
            .WithSummary("Updates a tenant.")
            .WithDescription("Updates mutable tenant settings. Database settings can only be changed while a tenant is uninitialized.")
            .WithCliCommand(new CliOperationMetadata(["tenants"], "update")
            {
                Capability = TenantManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("tenantName", 0) },
                InputMode = CliInputMode.Json,
            })
            .Accepts<TenantUpdateRequest>("application/json")
            .Produces<TenantResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        builder.MapManagementDelete(RoutePrefix + "/{tenantName}", DeleteAsync)
            .WithName("ApiDeleteTenant")
            .WithSummary("Deletes a tenant.")
            .WithDescription("Deletes a disabled or uninitialized tenant when tenant removal is enabled.")
            .WithCliCommand(new CliOperationMetadata(["tenants"], "delete")
            {
                Capability = TenantManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("tenantName", 0) },
                RequiresConfirmation = true,
            })
            .Produces<TenantOperationResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        builder.MapManagementPost(RoutePrefix + "/{tenantName}:start", StartAsync)
            .WithName("ApiStartTenant")
            .WithSummary("Starts a tenant.")
            .WithDescription("Transitions a disabled tenant back to the running state.")
            .WithCliCommand(new CliOperationMetadata(["tenants"], "start")
            {
                Capability = TenantManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("tenantName", 0) },
            })
            .Produces<TenantResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        builder.MapManagementPost(RoutePrefix + "/{tenantName}:stop", StopAsync)
            .WithName("ApiStopTenant")
            .WithSummary("Stops a tenant.")
            .WithDescription("Transitions a running tenant to the disabled state.")
            .WithCliCommand(new CliOperationMetadata(["tenants"], "stop")
            {
                Capability = TenantManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("tenantName", 0) },
                RequiresConfirmation = true,
            })
            .Produces<TenantResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        builder.MapManagementPost(RoutePrefix + "/{tenantName}:enable-remote-management", EnableRemoteManagementAsync)
            .WithName("ApiEnableTenantRemoteManagement")
            .WithSummary("Enables remote management for a tenant.")
            .WithDescription("Enables and configures the Remote Management CLI feature and OpenID Connect in a running tenant so the CLI can register its URL and authenticate directly.")
            .WithCliCommand(new CliOperationMetadata(["tenants"], "enable-remote-management")
            {
                Capability = TenantManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("tenantName", 0) },
            })
            .Produces<TenantOperationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return builder;
    }

    internal static async Task<IResult> EnableRemoteManagementAsync(
        HttpContext httpContext,
        string tenantName,
        [FromServices] IShellHost shellHost,
        [FromServices] ShellSettings currentShellSettings,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IStringLocalizer<TenantApiController> localizer,
        [FromQuery] bool provisionClient = false)
    {
        if (await AuthorizeManageTenantsAsync(httpContext, authorizationService, currentShellSettings, localizer) is { } authError)
        {
            return authError;
        }

        if (!shellHost.TryGetSettings(tenantName, out var settings))
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["Tenant not found: '{0}'.", tenantName]);
        }

        if (!settings.IsRunning())
        {
            return TypedResults.Problem(
                title: localizer["Conflict"],
                detail: localizer["Tenant '{0}' must be running before remote management can be enabled.", tenantName],
                statusCode: StatusCodes.Status409Conflict);
        }

        var credentials = provisionClient ? RemoteManagementClientCredentials.Generate() : null;
        if (!await RemoteManagementProvisioning.ConfigureAsync(shellHost, settings, credentials))
        {
            return httpContext.ApiBadRequestProblem(detail: localizer["Remote management could not be enabled for tenant '{0}'. Check its active feature profile.", tenantName]);
        }

        if (credentials is not null)
        {
            httpContext.Response.Headers.CacheControl = "no-store";
        }

        return TypedResults.Ok(new TenantOperationResponse
        {
            Name = settings.Name,
            Action = "enable-remote-management",
            ClientCredentials = credentials,
            State = settings.State.ToString(),
            Url = GetTenantUrls(httpContext, settings).FirstOrDefault(),
        });
    }

    internal static async Task<IResult> ListAsync(
        HttpContext httpContext,
        [FromServices] IShellHost shellHost,
        [FromServices] ShellSettings currentShellSettings,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IDataProtectionProvider dataProtectionProvider,
        [FromServices] IClock clock,
        [FromServices] IStringLocalizer<TenantApiController> localizer,
        [AsParameters] TenantListRequest request)
    {
        var skip = request.Skip ?? 0;
        var take = request.Take ?? 20;

        if (ValidatePaging(httpContext, skip, take, localizer) is { } pagingError)
        {
            return pagingError;
        }

        if (await AuthorizeManageTenantsAsync(httpContext, authorizationService, currentShellSettings, localizer) is { } authError)
        {
            return authError;
        }

        var items = shellHost.GetAllSettings()
            .Where(settings => MatchesFilter(settings, request))
            .OrderBy(settings => settings.Name, StringComparer.OrdinalIgnoreCase)
            .Select(settings => CreateTenantResponse(httpContext, settings, dataProtectionProvider, clock))
            .ToArray();

        var pagedItems = items
            .Skip(skip)
            .Take(take)
            .ToArray();

        return TypedResults.Ok(new TenantListResponse
        {
            Skip = skip,
            Take = take,
            TotalCount = items.Length,
            Items = pagedItems,
        });
    }

    internal static async Task<IResult> GetAsync(
        HttpContext httpContext,
        string tenantName,
        [FromServices] IShellHost shellHost,
        [FromServices] ShellSettings currentShellSettings,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IDataProtectionProvider dataProtectionProvider,
        [FromServices] IClock clock,
        [FromServices] IStringLocalizer<TenantApiController> localizer)
    {
        if (await AuthorizeManageTenantsAsync(httpContext, authorizationService, currentShellSettings, localizer) is { } authError)
        {
            return authError;
        }

        if (!shellHost.TryGetSettings(tenantName, out var shellSettings))
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["Tenant not found: '{0}'.", tenantName]);
        }

        return TypedResults.Ok(CreateTenantResponse(httpContext, shellSettings, dataProtectionProvider, clock));
    }

    internal static async Task<IResult> CreateAsync(
        HttpContext httpContext,
        [FromBody] TenantCreateRequest request,
        [FromServices] IShellHost shellHost,
        [FromServices] ShellSettings currentShellSettings,
        [FromServices] IShellSettingsManager shellSettingsManager,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IDataProtectionProvider dataProtectionProvider,
        [FromServices] IClock clock,
        [FromServices] IEnumerable<DatabaseProvider> databaseProviders,
        [FromServices] ITenantValidator tenantValidator,
        [FromServices] TenantDatabasePatternResolver tenantDatabasePatternResolver,
        [FromServices] IStringLocalizer<TenantApiController> localizer,
        [FromServices] ILogger<TenantApiController> logger,
        [FromServices] IDistributedLock distributedLock)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await AuthorizeManageTenantsAsync(httpContext, authorizationService, currentShellSettings, localizer) is { } authError)
        {
            return authError;
        }

        return await WithSetupLockAsync(request.Name, distributedLock, localizer, () =>
            CreateCoreAsync(httpContext, request, shellHost, shellSettingsManager, dataProtectionProvider, clock, databaseProviders, tenantValidator, tenantDatabasePatternResolver, localizer, logger));
    }

    private static async Task<IResult> CreateCoreAsync(
        HttpContext httpContext,
        TenantCreateRequest request,
        IShellHost shellHost,
        IShellSettingsManager shellSettingsManager,
        IDataProtectionProvider dataProtectionProvider,
        IClock clock,
        IEnumerable<DatabaseProvider> databaseProviders,
        ITenantValidator tenantValidator,
        TenantDatabasePatternResolver tenantDatabasePatternResolver,
        IStringLocalizer<TenantApiController> localizer,
        ILogger<TenantApiController> logger)
    {
        var apiModel = request.ToApiModel();
        ApplyConfiguredDatabasePatterns(apiModel, tenantDatabasePatternResolver);
        ApplyPresetDatabaseConfiguration(apiModel, shellSettingsManager, databaseProviders);

        if (shellHost.TryGetSettings(request.Name, out var existingSettings))
        {
            return MatchesCreateRequest(existingSettings, apiModel)
                ? TypedResults.Ok(CreateTenantResponse(httpContext, existingSettings, dataProtectionProvider, clock))
                : TypedResults.Problem(
                    title: localizer["Conflict"],
                    detail: localizer["Tenant '{0}' already exists with different settings.", request.Name],
                    statusCode: StatusCodes.Status409Conflict);
        }

        var validationState = new ModelStateDictionary();
        try
        {
            apiModel.IsNewTenant = true;
            validationState.AddModelErrors(await tenantValidator.ValidateAsync(apiModel));
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            logger.LogError(ex, "An error occurred while validating tenant '{TenantName}'.", request.Name);
            validationState.AddModelError(string.Empty, localizer["An error occurred while validating the tenant database settings."]);
        }

        if (!validationState.IsValid)
        {
            return httpContext.ApiValidationProblem(modelState: validationState);
        }

        try
        {
            using var shellSettings = shellSettingsManager.CreateDefaultSettings().AsUninitialized().AsDisposable();
            shellSettings.Name = apiModel.Name;
            shellSettings.RequestUrlHost = apiModel.RequestUrlHost;
            shellSettings.RequestUrlPrefix = apiModel.RequestUrlPrefix;
            shellSettings["Category"] = apiModel.Category;
            shellSettings["Description"] = apiModel.Description;
            shellSettings["ConnectionString"] = apiModel.ConnectionString;
            shellSettings["TablePrefix"] = apiModel.TablePrefix;
            shellSettings["Schema"] = apiModel.Schema;
            shellSettings["DatabaseProvider"] = apiModel.DatabaseProvider;
            shellSettings["Secret"] = Guid.NewGuid().ToString();
            shellSettings["RecipeName"] = apiModel.RecipeName;
            shellSettings["FeatureProfile"] = string.Join(',', apiModel.FeatureProfiles ?? []);

            await shellHost.UpdateShellSettingsAsync(shellSettings);

            var reloadedSettings = shellHost.GetSettings(shellSettings.Name);
            var response = CreateTenantResponse(httpContext, reloadedSettings, dataProtectionProvider, clock);
            return TypedResults.Created($"/{RoutePrefix}/{Uri.EscapeDataString(reloadedSettings.Name)}", response);
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            if (shellHost.TryGetSettings(request.Name, out var concurrentSettings))
            {
                return MatchesCreateRequest(concurrentSettings, apiModel)
                    ? TypedResults.Ok(CreateTenantResponse(httpContext, concurrentSettings, dataProtectionProvider, clock))
                    : TypedResults.Problem(
                        title: localizer["Conflict"],
                        detail: localizer["Tenant '{0}' already exists with different settings.", request.Name],
                        statusCode: StatusCodes.Status409Conflict);
            }

            logger.LogError(ex, "An error occurred while saving tenant '{TenantName}'.", request.Name);
            return httpContext.ApiBadRequestProblem(detail: localizer["An error occurred while saving the tenant settings."]);
        }
    }

    internal static async Task<IResult> UpdateAsync(
        HttpContext httpContext,
        string tenantName,
        [FromBody] TenantUpdateRequest request,
        [FromServices] IShellHost shellHost,
        [FromServices] ShellSettings currentShellSettings,
        [FromServices] IShellSettingsManager shellSettingsManager,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IDataProtectionProvider dataProtectionProvider,
        [FromServices] IClock clock,
        [FromServices] IEnumerable<DatabaseProvider> databaseProviders,
        [FromServices] ITenantValidator tenantValidator,
        [FromServices] TenantDatabasePatternResolver tenantDatabasePatternResolver,
        [FromServices] IStringLocalizer<TenantApiController> localizer,
        [FromServices] ILogger<TenantApiController> logger)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await AuthorizeManageTenantsAsync(httpContext, authorizationService, currentShellSettings, localizer) is { } authError)
        {
            return authError;
        }

        if (!shellHost.TryGetSettings(tenantName, out var shellSettings))
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["Tenant not found: '{0}'.", tenantName]);
        }

        var apiModel = request.ToApiModel(shellSettings.Name);
        apiModel.ConnectionString = TenantConnectionStringRedactor.RestoreIfRedacted(shellSettings["ConnectionString"], apiModel.ConnectionString);

        ApplyConfiguredDatabasePatterns(apiModel, tenantDatabasePatternResolver);
        ApplyPresetDatabaseConfiguration(apiModel, shellSettingsManager, databaseProviders);

        var validationState = new ModelStateDictionary();
        try
        {
            apiModel.IsNewTenant = false;
            validationState.AddModelErrors(await tenantValidator.ValidateAsync(apiModel));
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            logger.LogError(ex, "An error occurred while validating tenant '{TenantName}'.", tenantName);
            validationState.AddModelError(string.Empty, localizer["An error occurred while validating the tenant database settings."]);
        }

        if (!validationState.IsValid)
        {
            return httpContext.ApiValidationProblem(modelState: validationState);
        }

        try
        {
            shellSettings["Description"] = apiModel.Description;
            shellSettings["Category"] = apiModel.Category;
            shellSettings.RequestUrlPrefix = apiModel.RequestUrlPrefix;
            shellSettings.RequestUrlHost = apiModel.RequestUrlHost;
            shellSettings["FeatureProfile"] = string.Join(',', apiModel.FeatureProfiles ?? []);

            if (shellSettings.IsUninitialized())
            {
                shellSettings["DatabaseProvider"] = apiModel.DatabaseProvider;
                shellSettings["TablePrefix"] = apiModel.TablePrefix;
                shellSettings["Schema"] = apiModel.Schema;
                shellSettings["ConnectionString"] = apiModel.ConnectionString;
                shellSettings["RecipeName"] = apiModel.RecipeName;
                shellSettings["Secret"] = shellSettings["Secret"] ?? Guid.NewGuid().ToString();
            }

            await shellHost.UpdateShellSettingsAsync(shellSettings);
            return TypedResults.Ok(CreateTenantResponse(httpContext, shellSettings, dataProtectionProvider, clock));
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            logger.LogError(ex, "An error occurred while saving tenant '{TenantName}'.", tenantName);
            return httpContext.ApiBadRequestProblem(detail: localizer["An error occurred while saving the tenant settings."]);
        }
    }

    internal static async Task<IResult> SetupAsync(
        HttpContext httpContext,
        string tenantName,
        [FromBody] TenantSetupRequest request,
        [FromServices] IShellHost shellHost,
        [FromServices] ShellSettings currentShellSettings,
        [FromServices] IShellSettingsManager shellSettingsManager,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IDataProtectionProvider dataProtectionProvider,
        [FromServices] IClock clock,
        [FromServices] ISetupService setupService,
        [FromServices] IEmailAddressValidator emailAddressValidator,
        [FromServices] IOptions<IdentityOptions> identityOptions,
        [FromServices] IEnumerable<DatabaseProvider> databaseProviders,
        [FromServices] TenantDatabasePatternResolver tenantDatabasePatternResolver,
        [FromServices] IDistributedLock distributedLock,
        [FromServices] IStringLocalizer<TenantApiController> localizer,
        [FromServices] ILogger<TenantApiController> logger)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await AuthorizeManageTenantsAsync(httpContext, authorizationService, currentShellSettings, localizer) is { } authError)
        {
            return authError;
        }

        return await WithSetupLockAsync(tenantName, distributedLock, localizer, () =>
            SetupCoreAsync(httpContext, tenantName, request, shellHost, shellSettingsManager, dataProtectionProvider, clock, setupService, emailAddressValidator, identityOptions, databaseProviders, tenantDatabasePatternResolver, localizer, logger));
    }

    private static async Task<IResult> SetupCoreAsync(
        HttpContext httpContext,
        string tenantName,
        TenantSetupRequest request,
        IShellHost shellHost,
        IShellSettingsManager shellSettingsManager,
        IDataProtectionProvider dataProtectionProvider,
        IClock clock,
        ISetupService setupService,
        IEmailAddressValidator emailAddressValidator,
        IOptions<IdentityOptions> identityOptions,
        IEnumerable<DatabaseProvider> databaseProviders,
        TenantDatabasePatternResolver tenantDatabasePatternResolver,
        IStringLocalizer<TenantApiController> localizer,
        ILogger<TenantApiController> logger)
    {
        if (!shellHost.TryGetSettings(tenantName, out var shellSettings))
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["Tenant not found: '{0}'.", tenantName]);
        }

        if (shellSettings.IsRunning())
        {
            return TypedResults.Ok(CreateTenantResponse(httpContext, shellSettings, dataProtectionProvider, clock));
        }

        if (!shellSettings.IsUninitialized())
        {
            return TypedResults.Problem(
                title: localizer["Conflict"],
                detail: localizer["Tenant '{0}' cannot be set up while it is in the '{1}' state.", tenantName, shellSettings.State],
                statusCode: StatusCodes.Status409Conflict);
        }

        var validationState = new ModelStateDictionary();
        if (string.IsNullOrWhiteSpace(request.SiteName))
        {
            validationState.AddModelError(nameof(request.SiteName), localizer["The site name is required."]);
        }

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            validationState.AddModelError(nameof(request.UserName), localizer["The user name is required."]);
        }
        else if (request.UserName.Any(character => !identityOptions.Value.User.AllowedUserNameCharacters.Contains(character)))
        {
            validationState.AddModelError(nameof(request.UserName), localizer["User name '{0}' is invalid, can only contain letters or digits.", request.UserName]);
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            validationState.AddModelError(nameof(request.Email), localizer["The email is required."]);
        }
        else if (!emailAddressValidator.Validate(request.Email))
        {
            validationState.AddModelError(nameof(request.Email), localizer["The email is invalid."]);
        }

        ValidateSetupPassword(request.Password, identityOptions.Value.Password, validationState, localizer);

        if (!validationState.IsValid)
        {
            return httpContext.ApiValidationProblem(modelState: validationState);
        }

        var databaseProviderLookup = databaseProviders.ToDictionary(provider => provider.Value, StringComparer.OrdinalIgnoreCase);
        var databaseProvider = string.IsNullOrEmpty(shellSettings["DatabaseProvider"]) ? request.DatabaseProvider : shellSettings["DatabaseProvider"];
        var connectionString = string.IsNullOrEmpty(shellSettings["ConnectionString"]) ? request.ConnectionString : shellSettings["ConnectionString"];
        var tablePrefix = string.IsNullOrEmpty(shellSettings["TablePrefix"]) ? request.TablePrefix : shellSettings["TablePrefix"];
        var schema = string.IsNullOrEmpty(shellSettings["Schema"]) ? request.Schema : shellSettings["Schema"];

        var databasePatternResolution = tenantDatabasePatternResolver.Resolve(shellSettings);
        if (!string.IsNullOrEmpty(databasePatternResolution.TablePrefixError))
        {
            validationState.AddModelError(nameof(request.TablePrefix), databasePatternResolution.TablePrefixError);
        }
        else if (databasePatternResolution.HasTablePrefixPattern)
        {
            tablePrefix = databasePatternResolution.TablePrefix;
        }

        if (!string.IsNullOrEmpty(databasePatternResolution.SchemaError))
        {
            validationState.AddModelError(nameof(request.Schema), databasePatternResolution.SchemaError);
        }
        else if (databasePatternResolution.HasSchemaPattern)
        {
            schema = databasePatternResolution.Schema;
        }

        using (var defaultSettings = shellSettingsManager.CreateDefaultSettings().AsDisposable())
        {
            var presetProviderName = defaultSettings["DatabaseProvider"];
            if (!string.IsNullOrEmpty(presetProviderName) &&
                databaseProviderLookup.TryGetValue(presetProviderName, out var presetProvider))
            {
                databaseProvider = presetProviderName;
                if (!presetProvider.HasConnectionString || !string.IsNullOrWhiteSpace(defaultSettings["ConnectionString"]))
                {
                    connectionString = defaultSettings["ConnectionString"];
                    schema = defaultSettings["Schema"];
                }
            }
        }

        if (string.IsNullOrEmpty(databaseProvider))
        {
            validationState.AddModelError(nameof(request.DatabaseProvider), localizer["The database provider is required."]);
        }
        else if (!databaseProviderLookup.TryGetValue(databaseProvider, out var selectedProvider))
        {
            validationState.AddModelError(nameof(request.DatabaseProvider), localizer["The database provider is not supported."]);
        }
        else
        {
            databaseProvider = selectedProvider.Value;
            if (selectedProvider.HasConnectionString && string.IsNullOrWhiteSpace(connectionString))
            {
                validationState.AddModelError(nameof(request.ConnectionString), localizer["The connection string is required for this database provider."]);
            }
        }

        var recipeName = string.IsNullOrEmpty(shellSettings["RecipeName"]) ? request.RecipeName : shellSettings["RecipeName"];
        RecipeDescriptor recipe = null;
        if (string.IsNullOrWhiteSpace(recipeName))
        {
            validationState.AddModelError(nameof(request.RecipeName), localizer["The recipe name is required."]);
        }
        else
        {
            recipe = (await setupService.GetSetupRecipesAsync())
                .FirstOrDefault(candidate => string.Equals(candidate.Name, recipeName, StringComparison.OrdinalIgnoreCase));
            if (recipe is null)
            {
                validationState.AddModelError(nameof(request.RecipeName), localizer["Recipe '{0}' not found.", recipeName]);
            }
        }

        if (!validationState.IsValid)
        {
            return httpContext.ApiValidationProblem(modelState: validationState);
        }

        var setupContext = new SetupContext
        {
            ShellSettings = shellSettings,
            EnabledFeatures = null,
            Errors = new Dictionary<string, string>(),
            Recipe = recipe,
            Properties = new Dictionary<string, object>
            {
                [SetupConstants.SiteName] = request.SiteName,
                [SetupConstants.AdminUsername] = request.UserName,
                [SetupConstants.AdminEmail] = request.Email,
                [SetupConstants.AdminPassword] = request.Password,
                [SetupConstants.SiteTimeZone] = string.IsNullOrWhiteSpace(request.SiteTimeZone)
                    ? clock.GetSystemTimeZone().TimeZoneId
                    : request.SiteTimeZone,
                [SetupConstants.DatabaseProvider] = databaseProvider,
                [SetupConstants.DatabaseConnectionString] = connectionString,
                [SetupConstants.DatabaseTablePrefix] = tablePrefix,
                [SetupConstants.DatabaseSchema] = schema,
            },
        };

        try
        {
            _ = await setupService.SetupAsync(setupContext);
        }
        catch (Exception exception) when (!exception.IsFatal())
        {
            logger.LogError(exception, "An error occurred while setting up tenant '{TenantName}'.", tenantName);
            return TypedResults.Problem(
                title: localizer["Tenant setup failed"],
                detail: localizer["An error occurred while running the setup. Consult the application logs for details."],
                statusCode: StatusCodes.Status500InternalServerError);
        }

        if (setupContext.Errors.Count > 0)
        {
            foreach (var error in setupContext.Errors)
            {
                validationState.AddModelError(error.Key, error.Value);
            }

            return httpContext.ApiValidationProblem(modelState: validationState);
        }

        _ = shellHost.TryGetSettings(tenantName, out var updatedSettings);
        return TypedResults.Ok(CreateTenantResponse(httpContext, updatedSettings ?? shellSettings, dataProtectionProvider, clock));
    }

    internal static async Task<IResult> StartAsync(
        HttpContext httpContext,
        string tenantName,
        [FromServices] IShellHost shellHost,
        [FromServices] ShellSettings currentShellSettings,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IDataProtectionProvider dataProtectionProvider,
        [FromServices] IClock clock,
        [FromServices] IStringLocalizer<TenantApiController> localizer)
    {
        if (await AuthorizeManageTenantsAsync(httpContext, authorizationService, currentShellSettings, localizer) is { } authError)
        {
            return authError;
        }

        if (!shellHost.TryGetSettings(tenantName, out var shellSettings))
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["Tenant not found: '{0}'.", tenantName]);
        }

        if (!shellSettings.IsDisabled())
        {
            if (shellSettings.IsRunning())
            {
                return TypedResults.Ok(CreateTenantResponse(httpContext, shellSettings, dataProtectionProvider, clock));
            }

            return httpContext.ApiBadRequestProblem(detail: localizer["You can only start a disabled tenant."]);
        }

        await shellHost.UpdateShellSettingsAsync(shellSettings.AsRunning());
        return TypedResults.Ok(CreateTenantResponse(httpContext, shellSettings, dataProtectionProvider, clock));
    }

    internal static async Task<IResult> StopAsync(
        HttpContext httpContext,
        string tenantName,
        [FromServices] IShellHost shellHost,
        [FromServices] ShellSettings currentShellSettings,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IDataProtectionProvider dataProtectionProvider,
        [FromServices] IClock clock,
        [FromServices] IStringLocalizer<TenantApiController> localizer)
    {
        if (await AuthorizeManageTenantsAsync(httpContext, authorizationService, currentShellSettings, localizer) is { } authError)
        {
            return authError;
        }

        if (!shellHost.TryGetSettings(tenantName, out var shellSettings))
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["Tenant not found: '{0}'.", tenantName]);
        }

        if (!shellSettings.IsRunning())
        {
            if (shellSettings.IsDisabled())
            {
                return TypedResults.Ok(CreateTenantResponse(httpContext, shellSettings, dataProtectionProvider, clock));
            }

            return httpContext.ApiBadRequestProblem(detail: localizer["You can only stop a running tenant."]);
        }

        await shellHost.UpdateShellSettingsAsync(shellSettings.AsDisabled());
        return TypedResults.Ok(CreateTenantResponse(httpContext, shellSettings, dataProtectionProvider, clock));
    }

    internal static async Task<IResult> DeleteAsync(
        HttpContext httpContext,
        string tenantName,
        [FromServices] IShellHost shellHost,
        [FromServices] ShellSettings currentShellSettings,
        [FromServices] IShellRemovalManager shellRemovalManager,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IOptions<TenantsOptions> tenantsOptions,
        [FromServices] IStringLocalizer<TenantApiController> localizer,
        [FromServices] ILogger<TenantApiController> logger)
    {
        if (await AuthorizeManageTenantsAsync(httpContext, authorizationService, currentShellSettings, localizer) is { } authError)
        {
            return authError;
        }

        if (!tenantsOptions.Value.TenantRemovalAllowed)
        {
            return httpContext.ApiForbidProblem();
        }

        if (!shellHost.TryGetSettings(tenantName, out var shellSettings))
        {
            return TypedResults.NoContent();
        }

        if (!shellSettings.IsRemovable())
        {
            return httpContext.ApiBadRequestProblem(detail: localizer["You can only delete a disabled or uninitialized tenant."]);
        }

        var context = await shellRemovalManager.RemoveAsync(shellSettings);
        if (!context.Success)
        {
            return TypedResults.Problem(
                title: localizer["An error occurred while deleting tenant '{0}'.", tenantName],
                detail: context.ErrorMessage,
                statusCode: StatusCodes.Status400BadRequest);
        }

        logger.LogWarning("The tenant '{TenantName}' was removed.", shellSettings.Name);

        return TypedResults.Ok(new TenantOperationResponse
        {
            Name = tenantName,
            Action = "delete",
            State = nameof(TenantState.Disabled),
        });
    }

    internal static bool MatchesCreateRequest(ShellSettings settings, TenantApiModel model)
        => string.Equals(settings.Name, model.Name, StringComparison.OrdinalIgnoreCase)
            && Equal(settings.RequestUrlHost, model.RequestUrlHost, StringComparison.OrdinalIgnoreCase)
            && Equal(settings.RequestUrlPrefix, model.RequestUrlPrefix, StringComparison.OrdinalIgnoreCase)
            && Equal(settings["Category"], model.Category, StringComparison.Ordinal)
            && Equal(settings["Description"], model.Description, StringComparison.Ordinal)
            && Equal(settings["DatabaseProvider"], model.DatabaseProvider, StringComparison.OrdinalIgnoreCase)
            && Equal(settings["ConnectionString"], model.ConnectionString, StringComparison.Ordinal)
            && Equal(settings["TablePrefix"], model.TablePrefix, StringComparison.Ordinal)
            && Equal(settings["Schema"], model.Schema, StringComparison.Ordinal)
            && Equal(settings["RecipeName"], model.RecipeName, StringComparison.OrdinalIgnoreCase)
            && ReadFeatureProfiles(settings).Order(StringComparer.OrdinalIgnoreCase)
                .SequenceEqual((model.FeatureProfiles ?? []).Order(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);

    private static bool Equal(string left, string right, StringComparison comparison)
        => string.Equals(left ?? string.Empty, right ?? string.Empty, comparison);

    internal static TenantResponse CreateTenantResponse(HttpContext httpContext, ShellSettings shellSettings, IDataProtectionProvider dataProtectionProvider, IClock clock)
    {
        var urls = GetTenantUrls(httpContext, shellSettings);
        var setupUrl = shellSettings.IsUninitialized()
            ? BuildSetupUrl(httpContext, shellSettings, dataProtectionProvider, clock)
            : null;

        return new TenantResponse
        {
            Name = shellSettings.Name,
            TenantId = shellSettings.TenantId,
            State = shellSettings.State.ToString(),
            Category = shellSettings["Category"],
            Description = shellSettings["Description"],
            RequestUrlHost = shellSettings.RequestUrlHost,
            RequestUrlPrefix = shellSettings.RequestUrlPrefix,
            DatabaseProvider = shellSettings["DatabaseProvider"],
            ConnectionString = TenantConnectionStringRedactor.RedactPassword(shellSettings["ConnectionString"]),
            TablePrefix = shellSettings["TablePrefix"],
            Schema = shellSettings["Schema"],
            RecipeName = shellSettings["RecipeName"],
            FeatureProfiles = ReadFeatureProfiles(shellSettings),
            Urls = urls,
            PrimaryUrl = urls.FirstOrDefault(),
            SetupUrl = setupUrl,
            CanStart = shellSettings.IsDisabled(),
            CanStop = shellSettings.IsRunning(),
            CanDelete = shellSettings.IsRemovable(),
        };
    }

    private static async Task<IResult> AuthorizeManageTenantsAsync(HttpContext httpContext, IAuthorizationService authorizationService, ShellSettings currentShellSettings, IStringLocalizer<TenantApiController> localizer)
    {
        if (!currentShellSettings.IsDefaultShell())
        {
            return TypedResults.Problem(title: localizer["Forbidden"], detail: localizer["Only the Default tenant can manage tenants."], statusCode: StatusCodes.Status403Forbidden);
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, Permissions.ManageTenants))
        {
            return httpContext.ApiForbidProblem();
        }

        return null;
    }

    private static IResult ValidatePaging(HttpContext httpContext, int skip, int take, IStringLocalizer<TenantApiController> localizer)
    {
        if (skip < 0 || take < 1)
        {
            return httpContext.ApiBadRequestProblem(detail: localizer["Skip must be zero or greater and take must be greater than zero."]);
        }

        if (take > 200)
        {
            return httpContext.ApiBadRequestProblem(detail: localizer["Take cannot exceed 200."]);
        }

        return null;
    }

    private static bool MatchesFilter(ShellSettings settings, TenantListRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Search) &&
            !settings.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase) &&
            !(settings["Description"]?.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ?? false))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.Category) &&
            !string.Equals(settings["Category"], request.Category, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (request.State.HasValue && settings.State != request.State.Value)
        {
            return false;
        }

        return true;
    }

    private static void ApplyConfiguredDatabasePatterns(TenantModelBase model, TenantDatabasePatternResolver tenantDatabasePatternResolver)
    {
        if (!string.IsNullOrWhiteSpace(model.Name))
        {
            tenantDatabasePatternResolver.Apply(model);
        }
    }

    private static void ApplyPresetDatabaseConfiguration(TenantModelBase model, IShellSettingsManager shellSettingsManager, IEnumerable<DatabaseProvider> databaseProviders)
    {
        var providerLookup = databaseProviders.ToDictionary(provider => provider.Value, StringComparer.OrdinalIgnoreCase);
        using var defaultSettings = shellSettingsManager.CreateDefaultSettings().AsDisposable();
        var databaseProvider = defaultSettings["DatabaseProvider"];
        var connectionString = defaultSettings["ConnectionString"];

        if (string.IsNullOrEmpty(databaseProvider) || !providerLookup.TryGetValue(databaseProvider, out var provider))
        {
            return;
        }

        model.DatabaseProvider = databaseProvider;
        if (!provider.HasConnectionString || !string.IsNullOrWhiteSpace(connectionString))
        {
            model.ConnectionString = connectionString;
            model.Schema = defaultSettings["Schema"];
        }
    }

    private static string[] ReadFeatureProfiles(ShellSettings shellSettings)
        => (shellSettings["FeatureProfile"] ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string[] GetTenantUrls(HttpContext httpContext, ShellSettings shellSettings)
    {
        var hosts = shellSettings.RequestUrlHosts
            .Where(IsConcreteHost)
            .ToList();

        if (hosts.Count == 0 && !string.IsNullOrEmpty(httpContext.Request.Host.Host))
        {
            hosts.Add(httpContext.Request.Host.Value);
        }

        if (hosts.Count == 0)
        {
            return [];
        }

        var pathBase = httpContext.Features.Get<ShellContextFeature>()?.OriginalPathBase ?? PathString.Empty;
        if (!string.IsNullOrEmpty(shellSettings.RequestUrlPrefix))
        {
            pathBase = pathBase.Add('/' + shellSettings.RequestUrlPrefix);
        }

        return hosts
            .Select(host => $"{httpContext.Request.Scheme}://{new HostString(host) + pathBase}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string BuildSetupUrl(HttpContext httpContext, ShellSettings shellSettings, IDataProtectionProvider dataProtectionProvider, IClock clock)
    {
        var primaryUrl = GetTenantUrls(httpContext, shellSettings).FirstOrDefault();
        var secret = shellSettings["Secret"];

        if (string.IsNullOrEmpty(primaryUrl) || string.IsNullOrEmpty(secret))
        {
            return null;
        }

        var dataProtector = dataProtectionProvider.CreateProtector("Tokens").ToTimeLimitedDataProtector();
        var token = dataProtector.Protect(secret, clock.UtcNow.Add(TimeSpan.FromHours(24)));
        return primaryUrl + QueryString.Create("token", token);
    }

    private static bool IsConcreteHost(string host)
        => !string.IsNullOrWhiteSpace(host)
            && !host.Contains('*', StringComparison.Ordinal)
            && !host.Contains('{', StringComparison.Ordinal)
            && !host.Contains('}', StringComparison.Ordinal);

    internal sealed class TenantListRequest
    {
        public int? Skip { get; init; }
        public int? Take { get; init; }
        public string Search { get; init; }
        public string Category { get; init; }
        public TenantState? State { get; init; }
    }

    internal sealed class TenantCreateRequest
    {
        public string Name { get; init; } = string.Empty;
        public string RequestUrlHost { get; init; }
        public string RequestUrlPrefix { get; init; }
        public string Category { get; init; }
        public string Description { get; init; }
        public string DatabaseProvider { get; init; }
        public string ConnectionString { get; init; }
        public string TablePrefix { get; init; }
        public string Schema { get; init; }
        public string RecipeName { get; init; }
        public string[] FeatureProfiles { get; init; } = [];

        public TenantApiModel ToApiModel() => new()
        {
            Name = Name,
            RequestUrlHost = RequestUrlHost,
            RequestUrlPrefix = RequestUrlPrefix,
            Category = Category,
            Description = Description,
            DatabaseProvider = DatabaseProvider,
            ConnectionString = ConnectionString,
            TablePrefix = TablePrefix,
            Schema = Schema,
            RecipeName = RecipeName,
            FeatureProfiles = FeatureProfiles,
        };
    }

    internal sealed class TenantUpdateRequest
    {
        public string RequestUrlHost { get; init; }
        public string RequestUrlPrefix { get; init; }
        public string Category { get; init; }
        public string Description { get; init; }
        public string DatabaseProvider { get; init; }
        public string ConnectionString { get; init; }
        public string TablePrefix { get; init; }
        public string Schema { get; init; }
        public string RecipeName { get; init; }
        public string[] FeatureProfiles { get; init; } = [];

        public TenantApiModel ToApiModel(string name) => new()
        {
            Name = name,
            RequestUrlHost = RequestUrlHost,
            RequestUrlPrefix = RequestUrlPrefix,
            Category = Category,
            Description = Description,
            DatabaseProvider = DatabaseProvider,
            ConnectionString = ConnectionString,
            TablePrefix = TablePrefix,
            Schema = Schema,
            RecipeName = RecipeName,
            FeatureProfiles = FeatureProfiles,
        };
    }

    internal sealed class TenantSetupRequest
    {
        [Required]
        [Description("The site name.")]
        public string SiteName { get; init; }

        [Required]
        [Description("The initial administrator user name.")]
        public string UserName { get; init; }

        [Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        [Description("The initial administrator email address.")]
        public string Email { get; init; }

        [Required]
        [DataType(DataType.Password)]
        [Description("The initial administrator password.")]
        public string Password { get; init; }

        [Description("The setup recipe name. The tenant's configured recipe takes precedence.")]
        public string RecipeName { get; init; }

        [Description("The site time zone. Defaults to the server time zone.")]
        public string SiteTimeZone { get; init; }

        [Description("The database provider. The tenant's configured provider takes precedence.")]
        public string DatabaseProvider { get; init; }

        [Description("The database connection string. The tenant's configured connection string takes precedence.")]
        public string ConnectionString { get; init; }

        [Description("The database table prefix. The tenant's configured prefix takes precedence.")]
        public string TablePrefix { get; init; }

        [Description("The database schema. The tenant's configured schema takes precedence.")]
        public string Schema { get; init; }
    }

    internal sealed class TenantListResponse
    {
        public int Skip { get; init; }
        public int Take { get; init; }
        public int TotalCount { get; init; }
        public TenantResponse[] Items { get; init; } = [];
    }

    internal sealed class TenantResponse
    {
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public RemoteManagementClientCredentials ClientCredentials { get; set; }

        public string Name { get; init; } = string.Empty;
        public string TenantId { get; init; }
        public string State { get; init; }
        public string Category { get; init; }
        public string Description { get; init; }
        public string RequestUrlHost { get; init; }
        public string RequestUrlPrefix { get; init; }
        public string DatabaseProvider { get; init; }
        public string ConnectionString { get; init; }
        public string TablePrefix { get; init; }
        public string Schema { get; init; }
        public string RecipeName { get; init; }
        public string[] FeatureProfiles { get; init; } = [];
        public string[] Urls { get; init; } = [];
        public string PrimaryUrl { get; init; }
        public string SetupUrl { get; init; }
        public bool CanStart { get; init; }
        public bool CanStop { get; init; }
        public bool CanDelete { get; init; }
    }

    internal sealed class TenantOperationResponse
    {
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public RemoteManagementClientCredentials ClientCredentials { get; init; }

        public string Name { get; init; } = string.Empty;
        public string Action { get; init; } = string.Empty;
        public string State { get; init; } = string.Empty;
        public string Url { get; init; }
    }
}

internal static class TenantManagementApiEndpointConventions
{
    public const string CapabilityName = "tenants";
    public const string TagName = "Tenants";

    public static RouteHandlerBuilder MapManagementGet(this IEndpointRouteBuilder builder, string pattern, Delegate handler)
        => builder.MapGet(pattern, handler)
            .WithTags(TagName)
            .DisableAntiforgery()
            .RequireAuthorization(CreateBearerPolicy());

    public static RouteHandlerBuilder MapManagementPost(this IEndpointRouteBuilder builder, string pattern, Delegate handler)
        => builder.MapPost(pattern, handler)
            .WithTags(TagName)
            .DisableAntiforgery()
            .RequireAuthorization(CreateBearerPolicy());

    public static RouteHandlerBuilder MapManagementPut(this IEndpointRouteBuilder builder, string pattern, Delegate handler)
        => builder.MapPut(pattern, handler)
            .WithTags(TagName)
            .DisableAntiforgery()
            .RequireAuthorization(CreateBearerPolicy());

    public static RouteHandlerBuilder MapManagementDelete(this IEndpointRouteBuilder builder, string pattern, Delegate handler)
        => builder.MapDelete(pattern, handler)
            .WithTags(TagName)
            .DisableAntiforgery()
            .RequireAuthorization(CreateBearerPolicy());

    private static Action<AuthorizationPolicyBuilder> CreateBearerPolicy()
        => static policy => policy
            .AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
            .AddRequirements(new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement))
            .RequireAuthenticatedUser();
}
