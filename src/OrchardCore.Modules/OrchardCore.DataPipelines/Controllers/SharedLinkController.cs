using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.DataPipelines.SharedLinks;
using OrchardCore.Modules;

namespace OrchardCore.DataPipelines.Controllers;

/// <summary>
/// Opens the shared links, for their signed-in recipients only, with the handler of their kind of resource. There is no
/// list of the links outside the admin: a recipient gets each link by email.
/// </summary>
[Authorize]
[Feature(DataPipelinesConstants.CoreFeatureId)]
public sealed class SharedLinkController : Controller
{
    private readonly SharedLinkManager _manager;

    public SharedLinkController(SharedLinkManager manager)
    {
        _manager = manager;
    }

    [HttpGet("SharedLinks/{linkId}")]
    public async Task<IActionResult> Open(string linkId, string token)
    {
        var (result, link) = await _manager.UseAsync(linkId, token, User, HttpContext.RequestAborted);

        switch (result)
        {
            // A wrong token doesn't tell whether the link exists.
            case SharedLinkAccessResult.NotFound or SharedLinkAccessResult.InvalidToken:
                return NotFound();

            case SharedLinkAccessResult.Expired or SharedLinkAccessResult.UsedUp:
                return StatusCode(StatusCodes.Status410Gone);

            case SharedLinkAccessResult.NotARecipient:
                return Forbid();
        }

        var handler = _manager.FindHandler(link.Kind);

        if (handler is null)
        {
            return NotFound();
        }

        return await handler.OpenAsync(link, HttpContext);
    }
}
