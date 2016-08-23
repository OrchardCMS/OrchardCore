using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Orchard.Environment.Shell;
using Orchard.Setup.Services;
using Orchard.Setup.ViewModels;
using YesSql.Core.Services;

namespace Orchard.Setup.Controllers
{
    public class SetupController : Controller
    {
        private readonly ISetupService _setupService;
        private readonly ShellSettings _shellSettings;
        private const string DefaultRecipe = "Default";

        public SetupController(
            ISetupService setupService,
            ShellSettings shellSettings,
            IStringLocalizer<SetupController> t)
        {
            _setupService = setupService;
            _shellSettings = shellSettings;

            T = t;
        }

        public IStringLocalizer T { get; set; }

        private ActionResult IndexViewResult(SetupViewModel model)
        {
            return View(model);
        }

        public ActionResult Index()
        {
            return IndexViewResult(new SetupViewModel
            {
                DatabaseProviders = GetDatabaseProviders()
            });
        }

        [HttpPost, ActionName("Index")]
        public async Task<ActionResult> IndexPOST(SetupViewModel model)
        {
            model.DatabaseProviders = GetDatabaseProviders();

            var selectedProvider = model.DatabaseProviders.FirstOrDefault(x => x.Value == model.DatabaseProvider);

            if (selectedProvider != null && selectedProvider.HasConnectionString && String.IsNullOrWhiteSpace(model.ConnectionString))
            {
                ModelState.AddModelError("ConnectionString", T["The connection string is mandatory for this provider"]);
            }

            if (String.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), T["The password is required"]);
            }

            if (model.Password != model.PasswordConfirmation)
            {
                ModelState.AddModelError(nameof(model.PasswordConfirmation), T["The password confirmation doesn't match the password."]);
            }

            if (!ModelState.IsValid)
            {
                return IndexViewResult(model);
            }

            var setupContext = new SetupContext
            {
                SiteName = model.SiteName,
                DatabaseProvider = model.DatabaseProvider,
                DatabaseConnectionString = model.ConnectionString,
                DatabaseTablePrefix = model.TablePrefix,
                EnabledFeatures = null, // default list,
                AdminUsername = model.AdminUserName,
                AdminEmail = model.AdminEmail,
                AdminPassword = model.Password,
                Errors = new Dictionary<string, string>()
            };

            var executionId = await _setupService.SetupAsync(setupContext);

            // Check if a component in the Setup failed
            if (setupContext.Errors.Any())
            {
                foreach (var error in setupContext.Errors)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }

                return IndexViewResult(model);
            }

            var urlPrefix = "";
            if (!String.IsNullOrWhiteSpace(_shellSettings.RequestUrlPrefix))
            {
                urlPrefix = _shellSettings.RequestUrlPrefix + "/";
            }

            return Redirect("~/" + urlPrefix);
        }

        private IEnumerable<DatabaseProviderEntry> GetDatabaseProviders()
        {
            return new List<DatabaseProviderEntry>
            {
                new DatabaseProviderEntry { Name = "Sql Server", Value = "SqlConnection", HasConnectionString = true },
                new DatabaseProviderEntry { Name = "Sql Lite", Value = "Sqlite", HasConnectionString = false },
            };
        }
    }
}