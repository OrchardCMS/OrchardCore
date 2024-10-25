using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.FileStorage.AmazonS3;
using OrchardCore.Media.AmazonS3.ViewModels;

namespace OrchardCore.Media.AmazonS3;

public sealed class AdminController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly AwsStorageOptions _options;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;

    public AdminController(
        IAuthorizationService authorizationService,
        IOptions<AwsStorageOptions> options,
        INotifier notifier,
        IHtmlLocalizer<AdminController> htmlLocalizer)
    {
        _authorizationService = authorizationService;
        _notifier = notifier;
        H = htmlLocalizer;
        _options = options.Value;
    }

    public async Task<IActionResult> Options()
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewAmazonS3MediaOptions))
        {
            return Forbid();
        }

        if (_options.Validate().Any())
        {
            await _notifier.ErrorAsync(H["The Amazon S3 Media feature is enabled, but it was not configured with appsettings.json."]);
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
