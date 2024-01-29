using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage.AmazonS3;
using OrchardCore.Media.AmazonS3.ViewModels;

namespace OrchardCore.Media.AmazonS3
{
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly AwsStorageOptions _options;

        public AdminController(
            IAuthorizationService authorizationService,
            IOptions<AwsStorageOptions> options)
        {
            _authorizationService = authorizationService;
            _options = options.Value;
        }

        public async Task<IActionResult> Options()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewAmazonS3MediaOptions))
            {
                return Forbid();
            }

            var model = new OptionsViewModel
            {
                BucketName = _options.BucketName,
                BasePath = _options.BasePath,
                CreateBucket = _options.CreateBucket
            };

            return View(model);
        }
    }
}
