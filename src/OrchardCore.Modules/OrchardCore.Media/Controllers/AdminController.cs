using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Environment.Shell;
using OrchardCore.Media.Core.Helpers;
using OrchardCore.Media.Hubs;
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

        var tusEnabled = HttpContext.RequestServices.IsMediaTusEnabled();
        var signalrEnabled = HttpContext.RequestServices.GetService<IHubContext<MediaHub>>() is not null;
        var shellSettings = HttpContext.RequestServices.GetRequiredService<ShellSettings>();
        var hostEnvironment = HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();

        var model = new MediaIndexViewModel
        {
            SiteId = shellSettings.TenantId,
            MaxFileSize = _mediaOptions.MaxFileSize,
            AllowedExtensions = string.Join(',', _mediaOptions.AllowedFileExtensions),
            TusEnabled = tusEnabled,
            SignalrEnabled = signalrEnabled,
            DebugEnabled = hostEnvironment.IsDevelopment(),
        };

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
