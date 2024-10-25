using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Media.Azure.ViewModels;
using OrchardCore.Modules;

namespace OrchardCore.Media.Azure;

[Feature("OrchardCore.Media.Azure.Storage")]
[Admin("MediaAzureBlob/{action}", "AzureBlob.{action}")]
public sealed class AdminController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly MediaBlobStorageOptions _options;

    public AdminController(
        IAuthorizationService authorizationService,
        IOptions<MediaBlobStorageOptions> options)
    {
        _authorizationService = authorizationService;
        _options = options.Value;
    }

    public async Task<IActionResult> Options()
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewAzureMediaOptions))
        {
            return Forbid();
        }

        var model = new OptionsViewModel
        {
            CreateContainer = _options.CreateContainer,
            ContainerName = _options.ContainerName,
            ConnectionString = _options.ConnectionString,
            BasePath = _options.BasePath
        };

        return View(model);
    }
}
