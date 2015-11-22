using Microsoft.AspNet.Mvc;
using Orchard.Localization;
using Orchard.Environment.Shell;
using Orchard.Setup.Services;
using Orchard.Setup.ViewModels;
using System;

namespace Orchard.Setup.Controllers
{
    public class SetupController : Controller
    {
        private readonly ISetupService _setupService;
        private readonly ShellSettings _shellSettings;
        private const string DefaultRecipe = "Default";

        public SetupController(ISetupService setupService,
            ShellSettings shellSettings)
        {
            _setupService = setupService;
            _shellSettings = shellSettings;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private ActionResult IndexViewResult(SetupViewModel model)
        {
            return View(model);
        }

        public ActionResult Index()
        {
            return IndexViewResult(new SetupViewModel
            {
            });
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(SetupViewModel model)
        {
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
                EnabledFeatures = null, // default list
            };

            var executionId = _setupService.Setup(setupContext);

            var urlPrefix = "";
            if (!String.IsNullOrWhiteSpace(_shellSettings.RequestUrlPrefix))
            {
                urlPrefix = _shellSettings.RequestUrlPrefix + "/";
            }

            // Redirect to the welcome page.
            // TODO: Redirect on the home page once we don't rely on Orchard.Demo
            return Redirect("~/" + urlPrefix + "home/index");
        }
    }
}