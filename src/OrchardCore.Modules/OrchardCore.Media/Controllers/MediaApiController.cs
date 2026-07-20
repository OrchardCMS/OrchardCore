using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.Controllers;

[ApiController]
[Route("api/media")]
[IgnoreAntiforgeryToken]
public class MediaApiController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly MediaOptions _mediaOptions;

    public MediaApiController(
        IAuthorizationService authorizationService,
        IOptions<MediaOptions> options)
    {
        _authorizationService = authorizationService;
        _mediaOptions = options.Value;
    }

    [Authorize]
    [Route("Options")]
    public async Task<IActionResult> Options()
    {
        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ViewMediaOptions))
        {
            return Forbid();
        }

        return View(_mediaOptions);
    }
}
