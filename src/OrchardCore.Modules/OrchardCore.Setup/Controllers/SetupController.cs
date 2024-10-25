using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Abstractions.Setup;
using OrchardCore.Data;
using OrchardCore.Email;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Recipes.Models;
using OrchardCore.Setup.Services;
using OrchardCore.Setup.ViewModels;

namespace OrchardCore.Setup.Controllers;

public sealed class SetupController : Controller
{
    private readonly IClock _clock;
    private readonly ISetupService _setupService;
    private readonly ShellSettings _shellSettings;
    private readonly IShellHost _shellHost;
    private readonly IdentityOptions _identityOptions;
    private readonly IEmailAddressValidator _emailAddressValidator;
    private readonly IEnumerable<DatabaseProvider> _databaseProviders;
    private readonly ILogger _logger;

    internal readonly IStringLocalizer S;

    public SetupController(
        IClock clock,
        ISetupService setupService,
        ShellSettings shellSettings,
        IShellHost shellHost,
        IOptions<IdentityOptions> identityOptions,
        IEmailAddressValidator emailAddressValidator,
        IEnumerable<DatabaseProvider> databaseProviders,
        IStringLocalizer<SetupController> localizer,
        ILogger<SetupController> logger)
    {
        _clock = clock;
        _setupService = setupService;
        _shellSettings = shellSettings;
        _shellHost = shellHost;
        _identityOptions = identityOptions.Value;
        _emailAddressValidator = emailAddressValidator;
        _databaseProviders = databaseProviders;
        _logger = logger;
        S = localizer;
    }

    public async Task<ActionResult> Index(string token)
    {
        var recipes = await _setupService.GetSetupRecipesAsync();
        var defaultRecipe = recipes.FirstOrDefault(x => x.Tags.Contains("default")) ?? recipes.FirstOrDefault();

        if (!await ShouldProceedWithTokenAsync(token))
        {
            return NotFound();
        }

        var model = new SetupViewModel
        {
            DatabaseProviders = _databaseProviders,
            Recipes = recipes,
            RecipeName = defaultRecipe?.Name,
            Secret = token,
        };

        CopyShellSettingsValues(model);

        if (!string.IsNullOrEmpty(_shellSettings["TablePrefix"]))
        {
            model.DatabaseConfigurationPreset = true;
            model.TablePrefix = _shellSettings["TablePrefix"];
        }

        if (!string.IsNullOrEmpty(_shellSettings["Schema"]))
        {
            model.DatabaseConfigurationPreset = true;
            model.Schema = _shellSettings["Schema"];
        }

        return View(model);
    }

    [HttpPost, ActionName("Index")]
    public async Task<ActionResult> IndexPOST(SetupViewModel model)
    {
        if (!await ShouldProceedWithTokenAsync(model.Secret))
        {
            return StatusCode(404);
        }

        model.DatabaseProviders = _databaseProviders;
        model.Recipes = await _setupService.GetSetupRecipesAsync();

        if (string.IsNullOrEmpty(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), S["The password is required."]);
        }

        if (model.Password != model.PasswordConfirmation)
        {
            ModelState.AddModelError(nameof(model.PasswordConfirmation), S["The password confirmation doesn't match the password."]);
        }

        RecipeDescriptor selectedRecipe = null;
        if (!string.IsNullOrEmpty(_shellSettings["RecipeName"]))
        {
            selectedRecipe = model.Recipes.FirstOrDefault(x => x.Name == _shellSettings["RecipeName"]);
            if (selectedRecipe == null)
            {
                ModelState.AddModelError(nameof(model.RecipeName), S["Invalid recipe."]);
            }
        }
        else if (string.IsNullOrEmpty(model.RecipeName) || (selectedRecipe = model.Recipes.FirstOrDefault(x => x.Name == model.RecipeName)) == null)
        {
            ModelState.AddModelError(nameof(model.RecipeName), S["Invalid recipe."]);
        }

        // Only add additional errors if attribute validation has passed.
        if (!string.IsNullOrEmpty(model.Email) && !_emailAddressValidator.Validate(model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), S["The email is invalid."]);
        }

        if (!string.IsNullOrEmpty(model.UserName) && model.UserName.Any(c => !_identityOptions.User.AllowedUserNameCharacters.Contains(c)))
        {
            ModelState.AddModelError(nameof(model.UserName), S["User name '{0}' is invalid, can only contain letters or digits.", model.UserName]);
        }

        if (!ModelState.IsValid)
        {
            CopyShellSettingsValues(model);
            return View(model);
        }

        var setupContext = new SetupContext
        {
            ShellSettings = _shellSettings,
            EnabledFeatures = null, // default list,
            Errors = new Dictionary<string, string>(),
            Recipe = selectedRecipe,
            Properties = new Dictionary<string, object>
            {
                { SetupConstants.SiteName, model.SiteName },
                { SetupConstants.AdminUsername, model.UserName },
                { SetupConstants.AdminEmail, model.Email },
                { SetupConstants.AdminPassword, model.Password },
                { SetupConstants.SiteTimeZone, model.SiteTimeZone },
            }
        };

        if (!string.IsNullOrEmpty(_shellSettings["ConnectionString"]))
        {
            model.DatabaseConfigurationPreset = true;
            setupContext.Properties[SetupConstants.DatabaseProvider] = _shellSettings["DatabaseProvider"];
            setupContext.Properties[SetupConstants.DatabaseConnectionString] = _shellSettings["ConnectionString"];
            setupContext.Properties[SetupConstants.DatabaseTablePrefix] = _shellSettings["TablePrefix"];
            setupContext.Properties[SetupConstants.DatabaseSchema] = _shellSettings["Schema"];
        }
        else
        {
            setupContext.Properties[SetupConstants.DatabaseProvider] = model.DatabaseProvider;
            setupContext.Properties[SetupConstants.DatabaseConnectionString] = model.ConnectionString;
            setupContext.Properties[SetupConstants.DatabaseTablePrefix] = model.TablePrefix;
            setupContext.Properties[SetupConstants.DatabaseSchema] = model.Schema;
        }

        var executionId = await _setupService.SetupAsync(setupContext);

        // Check if any Setup component failed (e.g., database connection validation)
        if (setupContext.Errors.Count > 0)
        {
            foreach (var error in setupContext.Errors)
            {
                ModelState.AddModelError(error.Key, error.Value);
            }

            return View(model);
        }

        return Redirect("~/");
    }

    private void CopyShellSettingsValues(SetupViewModel model)
    {
        if (!string.IsNullOrEmpty(_shellSettings["ConnectionString"]))
        {
            model.DatabaseConfigurationPreset = true;
            model.ConnectionString = _shellSettings["ConnectionString"];
        }

        if (!string.IsNullOrEmpty(_shellSettings["RecipeName"]))
        {
            model.RecipeNamePreset = true;
            model.RecipeName = _shellSettings["RecipeName"];
        }

        if (!string.IsNullOrEmpty(_shellSettings["DatabaseProvider"]))
        {
            model.DatabaseConfigurationPreset = true;
            model.DatabaseProvider = _shellSettings["DatabaseProvider"];
        }
        else
        {
            model.DatabaseProvider = model.DatabaseProviders.FirstOrDefault(p => p.IsDefault)?.Value;
        }
    }

    private async Task<bool> ShouldProceedWithTokenAsync(string token)
    {
        if (!string.IsNullOrWhiteSpace(_shellSettings["Secret"]))
        {
            if (string.IsNullOrEmpty(token) || !await IsTokenValid(token))
            {
                _logger.LogWarning("An attempt to access '{TenantName}' without providing a secret was made", _shellSettings.Name);

                return false;
            }
        }

        return true;
    }

    private async Task<bool> IsTokenValid(string token)
    {
        var result = false;
        try
        {
            var shellScope = await _shellHost.GetScopeAsync(ShellSettings.DefaultShellName);

            await shellScope.UsingAsync(scope =>
            {
                var dataProtectionProvider = scope.ServiceProvider.GetRequiredService<IDataProtectionProvider>();
                var dataProtector = dataProtectionProvider.CreateProtector("Tokens").ToTimeLimitedDataProtector();

                var tokenValue = dataProtector.Unprotect(token, out var expiration);

                if (_clock.UtcNow < expiration.ToUniversalTime())
                {
                    if (_shellSettings["Secret"] == tokenValue)
                    {
                        result = true;
                    }
                }

                return Task.CompletedTask;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in decrypting the token");
        }

        return result;
    }
}
