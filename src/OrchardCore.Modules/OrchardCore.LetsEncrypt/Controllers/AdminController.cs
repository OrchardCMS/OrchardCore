using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.LetsEncrypt.Services;
using OrchardCore.LetsEncrypt.Settings;
using OrchardCore.LetsEncrypt.ViewModels;

namespace OrchardCore.LetsEncrypt.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAzureWebAppService _azureWebAppService;
        private readonly ICertificateManager _certificateManager;
        private readonly ILetsEncryptService _letsEncryptService;
        private readonly LetsEncryptAzureAuthSettings _azureAuthSettings;
        private readonly INotifier _notifier;

        public AdminController(
            IAzureWebAppService azureWebAppService,
            ICertificateManager certificateManager,
            ILetsEncryptService letsEncryptService,
            IOptions<LetsEncryptAzureAuthSettings> azureAuthSettings,
            INotifier notifier,
            IHtmlLocalizer<AdminController> localizer
            )
        {
            _azureWebAppService = azureWebAppService;
            _certificateManager = certificateManager;
            _letsEncryptService = letsEncryptService;
            _azureAuthSettings = azureAuthSettings.Value;
            _notifier = notifier;

            T = localizer;
        }

        public IHtmlLocalizer T { get; }

        public async Task<IActionResult> ManageCertificates()
        {
            return View(await BuildManageCertificatesViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> InstallCertificate(InstallCertificateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await BuildManageCertificatesViewModel();
                return View(model);
            }

            var viewModel = await _letsEncryptService.RequestHttpChallengeCertificate(model.RegistrationEmail, model.Hostnames, model.UseStaging);

            await _certificateManager.InstallAsync(viewModel);

            _notifier.Success(T["Successfully installed Let's Encrypt certificate!"]);

            return RedirectToAction("ManageCertificates");
        }

        // TODO: Need to change this as we don't need this much info on tenants. Also this will need to be more generic in the end to support non azure
        private async Task<ManageCertificatesViewModel> BuildManageCertificatesViewModel()
        {
            var model = new ManageCertificatesViewModel();

            var webApp = await _azureWebAppService.GetWebAppAsync();

            model.AvailableHostNames = webApp.HostNames;
            model.HostNameSslStates = webApp.HostNameSslStates;
            model.InstalledCertificates = await _azureWebAppService.GetAppServiceCertificatesAsync();

            return model;
        }
    }
}
