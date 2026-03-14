using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Media.Core.Helpers;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Controllers;

[Admin("Media/{action}", "Media.{action}")]
public sealed class AdminController : Controller
{
    private readonly IMediaFileStore _mediaFileStore;
    private readonly IAuthorizationService _authorizationService;
    private readonly MediaOptions _mediaOptions;

    internal readonly IStringLocalizer S;

    public AdminController(
        IMediaFileStore mediaFileStore,
        IAuthorizationService authorizationService,
        IOptions<MediaOptions> options,
        IStringLocalizer<AdminController> stringLocalizer)
    {
        _mediaFileStore = mediaFileStore;
        _authorizationService = authorizationService;
        _mediaOptions = options.Value;
        S = stringLocalizer;
    }

    [Admin("Media", "Media.Index")]
    public async Task<IActionResult> Index()
    {
        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMedia))
        {
            return Forbid();
        }

        return View();
    }

    public async Task<IActionResult> MediaApplication(MediaApplicationViewModel model)
    {
        // Check if the user has access to new folders. If not, we hide the "create folder" button from the root folder.
        model.AllowNewRootFolders = !HttpContext.IsSecureMediaEnabled() || await _authorizationService.AuthorizeAsync(User, MediaPermissions.ViewMedia, (object)"_non-existent-path-87FD1922-8F88-4A33-9766-DA03E6E6F7BA");

        return View(model);
    }

    public async Task<IActionResult> Options()
    {
        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ViewMediaOptions))
        {
            return Forbid();
        }

        return View(_mediaOptions);
    }

    public async Task<ActionResult<object>> GetPermittedStorage()
    {
        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMedia) ||
            !await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageMediaFolder, (object)string.Empty))
        {
            return Forbid();
        }

        var bytes = await _mediaFileStore.GetPermittedStorageAsync();
        var text = bytes == null ? S["Unspecified"] : FileSizeHelpers.FormatAsBytes(bytes.Value);

        return Ok(new { bytes, text });
    }
}
