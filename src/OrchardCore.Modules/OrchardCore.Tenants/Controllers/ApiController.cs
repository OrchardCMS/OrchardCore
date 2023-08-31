using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Abstractions.Setup;
using OrchardCore.Data;
using OrchardCore.Email;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Modules;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Recipes.Models;
using OrchardCore.Setup.Services;
using OrchardCore.Tenants.Models;
using OrchardCore.Tenants.Services;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Controllers
{
    [Route("api/tenants")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    public class ApiController : Controller
    {
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _currentShellSettings;
        private readonly IShellRemovalManager _shellRemovalManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IDataProtectionProvider _dataProtectorProvider;
        private readonly ISetupService _setupService;
        private readonly IClock _clock;
        private readonly IEmailAddressValidator _emailAddressValidator;
        private readonly IdentityOptions _identityOptions;
        private readonly TenantsOptions _tenantsOptions;
        private readonly IEnumerable<DatabaseProvider> _databaseProviders;
        private readonly ITenantValidator _tenantValidator;
        protected readonly IStringLocalizer S;
        private readonly ILogger _logger;

        public ApiController(
            IShellHost shellHost,
            ShellSettings currentShellSettings,
            IShellRemovalManager shellRemovalManager,
            IAuthorizationService authorizationService,
            IShellSettingsManager shellSettingsManager,
            IDataProtectionProvider dataProtectorProvider,
            ISetupService setupService,
            IClock clock,
            IEmailAddressValidator emailAddressValidator,
            IOptions<IdentityOptions> identityOptions,
            IOptions<TenantsOptions> tenantsOptions,
            IEnumerable<DatabaseProvider> databaseProviders,
            ITenantValidator tenantValidator,
            IStringLocalizer<ApiController> stringLocalizer,
            ILogger<ApiController> logger)
        {
            _shellHost = shellHost;
            _currentShellSettings = currentShellSettings;
            _shellRemovalManager = shellRemovalManager;
            _authorizationService = authorizationService;
            _dataProtectorProvider = dataProtectorProvider;
            _shellSettingsManager = shellSettingsManager;
            _setupService = setupService;
            _clock = clock;
            _emailAddressValidator = emailAddressValidator;
            _identityOptions = identityOptions.Value;
            _tenantsOptions = tenantsOptions.Value;
            _databaseProviders = databaseProviders;
            _tenantValidator = tenantValidator;
            S = stringLocalizer;
            _logger = logger;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create(TenantApiModel model)
        {
            if (!_currentShellSettings.IsDefaultShell())
            {
                return Forbid();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return this.ChallengeOrForbid("Api");
            }

            await ValidateModelAsync(model, isNewTenant: !_shellHost.TryGetSettings(model.Name, out var settings));

            if (ModelState.IsValid)
            {
                if (model.IsNewTenant)
                {
                    // Creates a default shell settings based on the configuration.
                    var shellSettings = _shellSettingsManager
                        .CreateDefaultSettings()
                        .AsUninitialized();

                    shellSettings.Name = model.Name;
                    shellSettings.RequestUrlHost = model.RequestUrlHost;
                    shellSettings.RequestUrlPrefix = model.RequestUrlPrefix;

                    shellSettings["Category"] = model.Category;
                    shellSettings["Description"] = model.Description;
                    shellSettings["ConnectionString"] = model.ConnectionString;
                    shellSettings["TablePrefix"] = model.TablePrefix;
                    shellSettings["Schema"] = model.Schema;
                    shellSettings["DatabaseProvider"] = model.DatabaseProvider;
                    shellSettings["Secret"] = Guid.NewGuid().ToString();
                    shellSettings["RecipeName"] = model.RecipeName;
                    shellSettings["FeatureProfile"] = String.Join(',', model.FeatureProfiles ?? Array.Empty<string>());

                    await _shellHost.UpdateShellSettingsAsync(shellSettings);

                    var token = CreateSetupToken(shellSettings);

                    return Ok(GetEncodedUrl(shellSettings, token));
                }
                else
                {
                    // Site already exists, return 201 for indempotency purposes.

                    var token = CreateSetupToken(settings);

                    return Created(GetEncodedUrl(settings, token), null);
                }
            }

            return BadRequest(ModelState);
        }

        [HttpPost]
        [Route("edit")]
        public async Task<IActionResult> Edit(TenantApiModel model)
        {
            if (!_currentShellSettings.IsDefaultShell())
            {
                return Forbid();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return this.ChallengeOrForbid("Api");
            }

            if (ModelState.IsValid)
            {
                await ValidateModelAsync(model, isNewTenant: false);
            }

            if (!_shellHost.TryGetSettings(model.Name, out var shellSettings))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                shellSettings["Description"] = model.Description;
                shellSettings["Category"] = model.Category;
                shellSettings.RequestUrlPrefix = model.RequestUrlPrefix;
                shellSettings.RequestUrlHost = model.RequestUrlHost;
                shellSettings["FeatureProfile"] = String.Join(',', model.FeatureProfiles ?? Array.Empty<string>());

                if (shellSettings.IsUninitialized())
                {
                    shellSettings["DatabaseProvider"] = model.DatabaseProvider;
                    shellSettings["TablePrefix"] = model.TablePrefix;
                    shellSettings["Schema"] = model.Schema;
                    shellSettings["ConnectionString"] = model.ConnectionString;
                    shellSettings["RecipeName"] = model.RecipeName;
                    shellSettings["Secret"] = Guid.NewGuid().ToString();
                }

                await _shellHost.UpdateShellSettingsAsync(shellSettings);

                return Ok();
            }

            return BadRequest(ModelState);
        }

        [HttpPost]
        [Route("disable/{tenantName}")]
        public async Task<IActionResult> Disable(string tenantName)
        {
            if (!_currentShellSettings.IsDefaultShell())
            {
                return Forbid();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return this.ChallengeOrForbid("Api");
            }

            if (!_shellHost.TryGetSettings(tenantName, out var shellSettings))
            {
                return NotFound();
            }

            if (!shellSettings.IsRunning())
            {
                return BadRequest(S["You can only disable a Running tenant."]);
            }

            await _shellHost.UpdateShellSettingsAsync(shellSettings.AsDisabled());

            return Ok();
        }

        [HttpPost]
        [Route("enable/{tenantName}")]
        public async Task<IActionResult> Enable(string tenantName)
        {
            if (!_currentShellSettings.IsDefaultShell())
            {
                return Forbid();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return this.ChallengeOrForbid("Api");
            }

            if (!_shellHost.TryGetSettings(tenantName, out var shellSettings))
            {
                return NotFound();
            }

            if (!shellSettings.IsDisabled())
            {
                return BadRequest(S["You can only enable a Disabled tenant."]);
            }

            await _shellHost.UpdateShellSettingsAsync(shellSettings.AsRunning());

            return Ok();
        }

        [HttpPost]
        [Route("remove/{tenantName}")]
        public async Task<IActionResult> Remove(string tenantName)
        {
            if (!_currentShellSettings.IsDefaultShell() || !_tenantsOptions.TenantRemovalAllowed)
            {
                return Forbid();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return this.ChallengeOrForbid("Api");
            }

            if (!_shellHost.TryGetSettings(tenantName, out var shellSettings))
            {
                return NotFound();
            }

            if (!shellSettings.IsRemovable())
            {
                return BadRequest(S["You can only remove a 'Disabled' or an 'Uninitialized' tenant."]);
            }

            var context = await _shellRemovalManager.RemoveAsync(shellSettings);
            if (!context.Success)
            {
                return Problem(
                    title: S["An error occurred while removing the tenant '{0}'.", tenantName],
                    detail: context.ErrorMessage,
                    statusCode: (int)HttpStatusCode.BadRequest);
            }

            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("The tenant '{TenantName}' was removed.", shellSettings.Name);
            }

            return Ok();
        }

        [HttpPost]
        [Route("setup")]
        public async Task<ActionResult> Setup(SetupApiViewModel model)
        {
            if (!_currentShellSettings.IsDefaultShell())
            {
                return this.ChallengeOrForbid("Api");
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenants))
            {
                return this.ChallengeOrForbid("Api");
            }

            if (!String.IsNullOrEmpty(model.UserName) && model.UserName.Any(c => !_identityOptions.User.AllowedUserNameCharacters.Contains(c)))
            {
                ModelState.AddModelError(nameof(model.UserName), S["User name '{0}' is invalid, can only contain letters or digits.", model.UserName]);
            }

            // Only add additional error if attribute validation has passed.
            if (!String.IsNullOrEmpty(model.Email) && !_emailAddressValidator.Validate(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), S["The email is invalid."]);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (!_shellHost.TryGetSettings(model.Name, out var shellSettings))
            {
                ModelState.AddModelError(nameof(SetupApiViewModel.Name), S["Tenant not found: '{0}'", model.Name]);
            }

            if (shellSettings.IsRunning())
            {
                return Created(GetEncodedUrl(shellSettings, null), null);
            }

            if (!shellSettings.IsUninitialized())
            {
                return BadRequest(S["The tenant can't be setup."]);
            }

            var databaseProvider = shellSettings["DatabaseProvider"];

            if (String.IsNullOrEmpty(databaseProvider))
            {
                databaseProvider = model.DatabaseProvider;
            }

            if (String.IsNullOrEmpty(databaseProvider))
            {
                return BadRequest(S["The database provider is not defined."]);
            }

            var selectedProvider = _databaseProviders.FirstOrDefault(provider => provider.Value == databaseProvider);
            if (selectedProvider == null)
            {
                return BadRequest(S["The database provider is not supported."]);
            }

            var tablePrefix = shellSettings["TablePrefix"];

            if (String.IsNullOrEmpty(tablePrefix))
            {
                tablePrefix = model.TablePrefix;
            }

            var schema = shellSettings["Schema"];

            if (String.IsNullOrEmpty(schema))
            {
                schema = model.Schema;
            }

            var connectionString = shellSettings["connectionString"];

            if (String.IsNullOrEmpty(connectionString))
            {
                connectionString = model.ConnectionString;
            }

            if (selectedProvider.HasConnectionString && String.IsNullOrEmpty(connectionString))
            {
                return BadRequest(S["The connection string is required for this database provider."]);
            }

            var recipeName = shellSettings["RecipeName"];

            if (String.IsNullOrEmpty(recipeName))
            {
                recipeName = model.RecipeName;
            }

            RecipeDescriptor recipeDescriptor = null;

            if (String.IsNullOrEmpty(recipeName))
            {
                if (model.Recipe == null)
                {
                    return BadRequest(S["Either 'Recipe' or 'RecipeName' is required."]);
                }

                var tempFilename = Path.GetTempFileName();

                using (var fs = System.IO.File.Create(tempFilename))
                {
                    await model.Recipe.CopyToAsync(fs);
                }

                var fileProvider = new PhysicalFileProvider(Path.GetDirectoryName(tempFilename));

                recipeDescriptor = new RecipeDescriptor
                {
                    FileProvider = fileProvider,
                    BasePath = String.Empty,
                    RecipeFileInfo = fileProvider.GetFileInfo(Path.GetFileName(tempFilename))
                };
            }
            else
            {
                var setupRecipes = await _setupService.GetSetupRecipesAsync();
                recipeDescriptor = setupRecipes.FirstOrDefault(x => String.Equals(x.Name, recipeName, StringComparison.OrdinalIgnoreCase));

                if (recipeDescriptor == null)
                {
                    return BadRequest(S["Recipe '{0}' not found.", recipeName]);
                }
            }

            var setupContext = new SetupContext
            {
                ShellSettings = shellSettings,
                EnabledFeatures = null, // default list,
                Errors = new Dictionary<string, string>(),
                Recipe = recipeDescriptor,
                Properties = new Dictionary<string, object>
                {
                    { SetupConstants.SiteName, model.SiteName },
                    { SetupConstants.AdminUsername, model.UserName },
                    { SetupConstants.AdminEmail, model.Email },
                    { SetupConstants.AdminPassword, model.Password },
                    { SetupConstants.SiteTimeZone, model.SiteTimeZone },
                    { SetupConstants.DatabaseProvider, selectedProvider.Value },
                    { SetupConstants.DatabaseConnectionString, connectionString },
                    { SetupConstants.DatabaseTablePrefix, tablePrefix },
                    { SetupConstants.DatabaseSchema, schema },
                }
            };

            var executionId = await _setupService.SetupAsync(setupContext);

            // Check if a component in the Setup failed.
            if (setupContext.Errors.Count > 0)
            {
                foreach (var error in setupContext.Errors)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }

                return this.InternalServerError(ModelState);
            }

            return Ok(executionId);
        }

        private string GetEncodedUrl(ShellSettings shellSettings, string token)
        {
            var host = shellSettings.RequestUrlHosts.FirstOrDefault();
            var hostString = host != null ? new HostString(host) : Request.Host;

            var pathString = HttpContext.Features.Get<ShellContextFeature>()?.OriginalPathBase ?? PathString.Empty;
            if (!String.IsNullOrEmpty(shellSettings.RequestUrlPrefix))
            {
                pathString = pathString.Add('/' + shellSettings.RequestUrlPrefix);
            }

            var queryString = QueryString.Empty;
            if (!String.IsNullOrEmpty(token))
            {
                queryString = QueryString.Create("token", token);
            }

            return $"{Request.Scheme}://{hostString + pathString + queryString}";
        }

        private string CreateSetupToken(ShellSettings shellSettings)
        {
            // Create a public url to setup the new tenant
            var dataProtector = _dataProtectorProvider.CreateProtector("Tokens").ToTimeLimitedDataProtector();
            var token = dataProtector.Protect(shellSettings["Secret"], _clock.UtcNow.Add(new TimeSpan(24, 0, 0)));

            return token;
        }

        private async Task ValidateModelAsync(TenantApiModel model, bool isNewTenant)
        {
            model.IsNewTenant = isNewTenant;

            ModelState.AddModelErrors(await _tenantValidator.ValidateAsync(model));
        }
    }
}
