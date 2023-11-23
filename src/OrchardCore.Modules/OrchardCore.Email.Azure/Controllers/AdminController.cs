using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Azure.ViewModels;

namespace OrchardCore.Email.Azure.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly AzureEmailSettings _options;

        public AdminController(IAuthorizationService authorizationService, IOptions<AzureEmailSettings> options)
        {
            _authorizationService = authorizationService;
            _options = options.Value;
        }

        public async Task<IActionResult> Options()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewAzureEmailOptions))
            {
                return Forbid();
            }

            var model = new OptionsViewModel
            {
                DefaultSender = _options.DefaultSender,
                ConnectionString = _options.ConnectionString
            };

            return View(model);
        }
    }
}
