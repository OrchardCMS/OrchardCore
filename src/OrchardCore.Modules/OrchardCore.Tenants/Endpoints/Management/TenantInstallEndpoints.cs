using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Data;
using OrchardCore.Email;
using OrchardCore.Environment.Shell;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Setup.Services;
using OrchardCore.RemoteManagement;
using OrchardCore.Tenants.Controllers;
using OrchardCore.Tenants.Services;

namespace OrchardCore.Tenants.Endpoints.Management;

internal static partial class TenantManagementEndpoints
{
    internal static async Task<IResult> InstallAsync(
        HttpContext httpContext,
        string tenantName,
        [FromBody] TenantInstallRequest request,
        [FromServices] IShellHost shellHost,
        [FromServices] IShellSettingsManager shellSettingsManager,
        [FromServices] IDataProtectionProvider dataProtectionProvider,
        [FromServices] IClock clock,
        [FromServices] IEnumerable<DatabaseProvider> databaseProviders,
        [FromServices] ITenantValidator tenantValidator,
        [FromServices] TenantDatabasePatternResolver tenantDatabasePatternResolver,
        [FromServices] IStringLocalizer<TenantApiController> localizer,
        [FromServices] ILogger<TenantApiController> logger,
        [FromServices] ISetupService setupService,
        [FromServices] IEmailAddressValidator emailAddressValidator,
        [FromServices] IOptions<IdentityOptions> identityOptions,
        [FromServices] ShellSettings currentShellSettings,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IDistributedLock distributedLock)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (await AuthorizeManageTenantsAsync(httpContext, authorizationService, currentShellSettings, localizer) is { } authError)
        {
            return authError;
        }

        return await WithSetupLockAsync(tenantName, distributedLock, localizer, async () =>
        {
            if (shellHost.TryGetSettings(tenantName, out _))
            {
                return TypedResults.Problem(
                    title: localizer["Conflict"],
                    detail: localizer["Tenant '{0}' already exists. Inspect it and use the separate setup command if it is still uninitialized.", tenantName],
                    statusCode: StatusCodes.Status409Conflict);
            }

            var validationState = new ModelStateDictionary();
            ValidateSetupPassword(request.Password, identityOptions.Value.Password, validationState, localizer);
            if (!validationState.IsValid)
            {
                return httpContext.ApiValidationProblem(modelState: validationState);
            }

            var created = await CreateCoreAsync(httpContext, request.ToCreateRequest(tenantName), shellHost, shellSettingsManager, dataProtectionProvider, clock, databaseProviders, tenantValidator, tenantDatabasePatternResolver, localizer, logger);
            if (created is not IStatusCodeHttpResult { StatusCode: StatusCodes.Status201Created })
            {
                return created is IStatusCodeHttpResult { StatusCode: StatusCodes.Status200OK }
                    ? TypedResults.Problem(statusCode: StatusCodes.Status409Conflict,
                        detail: localizer["Tenant '{0}' was created by another request. Inspect it before continuing.", tenantName])
                    : created;
            }

            var setup = await SetupCoreAsync(httpContext, tenantName, request.ToSetupRequest(), shellHost, shellSettingsManager, dataProtectionProvider, clock, setupService, emailAddressValidator, identityOptions, databaseProviders, tenantDatabasePatternResolver, localizer, logger);
            if (setup is IStatusCodeHttpResult { StatusCode: StatusCodes.Status200OK } &&
                setup is IValueHttpResult { Value: TenantResponse tenant })
            {
                if (request.EnableRemoteManagement)
                {
                    var credentials = RemoteManagementClientCredentials.Generate();
                    if (!shellHost.TryGetSettings(tenantName, out var settings) ||
                        !await RemoteManagementProvisioning.ConfigureAsync(shellHost, settings, credentials))
                    {
                        return TypedResults.Problem(statusCode: StatusCodes.Status409Conflict,
                            detail: localizer["The tenant was installed, but remote management could not be enabled. Check its feature profile, then use enable-remote-management with provisionClient=true."],
                            extensions: new Dictionary<string, object> { ["tenantName"] = tenantName, ["stage"] = "remote-management" });
                    }

                    tenant.ClientCredentials = credentials;
                    httpContext.Response.Headers.CacheControl = "no-store";
                }

                return TypedResults.Created($"/{RoutePrefix}/{Uri.EscapeDataString(tenantName)}", tenant);
            }

            var problem = setup switch
            {
                ProblemHttpResult failure => failure.ProblemDetails,
                ValidationProblem validation => validation.ProblemDetails,
                _ => null,
            };
            if (problem is not null)
            {
                problem.Extensions["tenantName"] = tenantName;
                problem.Extensions["stage"] = "setup";
                problem.Detail = localizer["The tenant was created, but setup did not complete. Inspect its state and use the separate setup command if it is uninitialized."]
                    + (string.IsNullOrEmpty(problem.Detail) ? string.Empty : " " + problem.Detail);
            }

            return setup;
        });
    }

    private static void ValidateSetupPassword(
        string password,
        PasswordOptions options,
        ModelStateDictionary validationState,
        IStringLocalizer<TenantApiController> localizer)
    {
        const string key = nameof(TenantInstallRequest.Password);
        if (string.IsNullOrWhiteSpace(password) || password.Length < options.RequiredLength)
        {
            validationState.AddModelError(key, localizer["Passwords must be at least {0} characters.", options.RequiredLength]);
        }

        password ??= string.Empty;
        if (options.RequireUppercase && !password.Any(char.IsAsciiLetterUpper))
        {
            validationState.AddModelError(key, localizer["Passwords must have at least one uppercase character ('A'-'Z')."]);
        }
        if (options.RequireLowercase && !password.Any(char.IsAsciiLetterLower))
        {
            validationState.AddModelError(key, localizer["Passwords must have at least one lowercase character ('a'-'z')."]);
        }
        if (options.RequireDigit && !password.Any(char.IsAsciiDigit))
        {
            validationState.AddModelError(key, localizer["Passwords must have at least one digit character ('0'-'9')."]);
        }
        if (options.RequireNonAlphanumeric && password.All(char.IsAsciiLetterOrDigit))
        {
            validationState.AddModelError(key, localizer["Passwords must have at least one non letter or digit character."]);
        }
        if (options.RequiredUniqueChars >= 1 && password.Distinct().Count() < options.RequiredUniqueChars)
        {
            validationState.AddModelError(key, localizer["Passwords must contain at least {0} unique characters.", options.RequiredUniqueChars]);
        }
    }

    private static async Task<IResult> WithSetupLockAsync(
        string tenantName,
        IDistributedLock distributedLock,
        IStringLocalizer<TenantApiController> localizer,
        Func<Task<IResult>> action)
    {
        (var locker, var locked) = await distributedLock.TryAcquireLockAsync(
            $"TENANT_SETUP_{tenantName?.ToUpperInvariant()}",
            TimeSpan.FromSeconds(3),
            TimeSpan.FromHours(1));
        if (!locked)
        {
            return TypedResults.Problem(
                title: localizer["Conflict"],
                detail: localizer["Tenant '{0}' is already being created or set up.", tenantName],
                statusCode: StatusCodes.Status409Conflict);
        }

        await using var setupLock = locker;
        return await action();
    }

    internal sealed class TenantInstallRequest
    {
        [Description("Enable Remote Management CLI and its OpenID dependencies, and return credentials for a new administrative application once.")]
        public bool EnableRemoteManagement { get; init; }

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

        [Description("The setup recipe name. Required unless configured by the host.")]
        public string RecipeName { get; init; }

        [Description("The site time zone. Defaults to the server time zone.")]
        public string SiteTimeZone { get; init; }

        [DefaultValue("Sqlite")]
        [Description("The database provider. Defaults to Sqlite. Host database presets take precedence.")]
        public string DatabaseProvider { get; init; } = "Sqlite";

        [Description("The database connection string. Host database presets take precedence.")]
        public string ConnectionString { get; init; }

        [Description("The database table prefix. Host database patterns take precedence.")]
        public string TablePrefix { get; init; }

        [Description("The database schema. Host database patterns take precedence.")]
        public string Schema { get; init; }

        public string RequestUrlHost { get; init; }
        public string RequestUrlPrefix { get; init; }
        public string Category { get; init; }
        public string Description { get; init; }
        public string[] FeatureProfiles { get; init; } = [];

        public TenantCreateRequest ToCreateRequest(string name) => new()
        {
            Name = name,
            RequestUrlHost = RequestUrlHost,
            RequestUrlPrefix = RequestUrlPrefix,
            Category = Category,
            Description = Description,
            FeatureProfiles = FeatureProfiles,
            DatabaseProvider = DatabaseProvider,
            ConnectionString = ConnectionString,
            TablePrefix = TablePrefix,
            Schema = Schema,
            RecipeName = RecipeName,
        };

        public TenantSetupRequest ToSetupRequest() => new()
        {
            SiteName = SiteName,
            UserName = UserName,
            Email = Email,
            Password = Password,
            SiteTimeZone = SiteTimeZone,
            RecipeName = RecipeName,
            DatabaseProvider = DatabaseProvider,
            ConnectionString = ConnectionString,
            TablePrefix = TablePrefix,
            Schema = Schema,
        };
    }
}
