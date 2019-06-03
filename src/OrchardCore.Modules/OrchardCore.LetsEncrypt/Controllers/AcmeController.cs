using Microsoft.AspNetCore.Mvc;
using OrchardCore.LetsEncrypt.Services;

namespace OrchardCore.LetsEncrypt.Controllers
{
    public class AcmeController : Controller
    {
        private readonly ILetsEncryptService _letsEncryptService;

        public AcmeController(ILetsEncryptService letsEncryptService)
        {
            _letsEncryptService = letsEncryptService;
        }

        [HttpGet]
        public IActionResult Challenge(string token)
        {
            var filename = _letsEncryptService.GetChallengeKeyFilename(token);

            return PhysicalFile(filename, "text/plain");
        }
    }
}
