using Microsoft.AspNet.Mvc;
using OrchardVNext.Abstractions.Localization;
using OrchardVNext.Configuration.Environment;
using OrchardVNext.Setup.Services;
using OrchardVNext.Setup.ViewModels;

namespace OrchardVNext.Setup.Controllers {
    public class SetupController : Controller {
        private readonly ISetupService _setupService;
        private readonly ShellSettings _shellSettings;
        private const string DefaultRecipe = "Default";

        public SetupController(ISetupService setupService,
            ShellSettings shellSettings) {
            _setupService = setupService;
            _shellSettings = shellSettings;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private ActionResult IndexViewResult(SetupViewModel model) {
            return View(model);
        }

        public ActionResult Index() {
            return IndexViewResult(new SetupViewModel {
            });
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(SetupViewModel model) {
            if (!ModelState.IsValid) {
                return IndexViewResult(model);
            }

            var setupContext = new SetupContext {
                SiteName = model.SiteName,
                EnabledFeatures = null, // default list
            };

            var executionId = _setupService.Setup(setupContext);

            // redirect to the welcome page.
            return Redirect("~/" + _shellSettings.RequestUrlPrefix + "home/index");
        }
    }
}
