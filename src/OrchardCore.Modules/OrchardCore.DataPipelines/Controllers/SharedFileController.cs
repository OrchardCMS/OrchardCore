using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.ViewModels;
using OrchardCore.Modules;
using ISession = YesSql.ISession;

namespace OrchardCore.DataPipelines.Controllers;

/// <summary>
/// Serves the files shared through download links, to their signed-in recipients only, and lists the files shared with
/// the signed-in user.
/// </summary>
[Authorize]
public sealed class SharedFileController : Controller
{
    private readonly DataPipelineSharedFileManager _manager;
    private readonly ISession _session;
    private readonly IClock _clock;

    public SharedFileController(DataPipelineSharedFileManager manager, ISession session, IClock clock)
    {
        _manager = manager;
        _session = session;
        _clock = clock;
    }

    [HttpGet("DataPipelines/Files")]
    public async Task<IActionResult> Index()
        => View(new DataPipelineSharedWithMeViewModel
        {
            Files = await _manager.ListSharedWithAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), _clock.UtcNow),
        });

    // A download from the list of the files shared with the user has no token; a link always has one.
    [HttpGet("DataPipelines/Files/{fileId}")]
    public async Task<IActionResult> Download(string fileId, string token)
    {
        var sharedFile = await _manager.GetAsync(fileId);
        var access = DataPipelineSharedFileAccess.Check(sharedFile, token, User, _clock.UtcNow);

        switch (access)
        {
            case DataPipelineSharedFileAccessResult.NotFound or DataPipelineSharedFileAccessResult.InvalidToken:
                return NotFound();

            case DataPipelineSharedFileAccessResult.Expired:
                return StatusCode(StatusCodes.Status410Gone);

            case DataPipelineSharedFileAccessResult.NotARecipient:
                return Forbid();
        }

        var stream = _manager.OpenRead(sharedFile);

        if (stream is null)
        {
            return NotFound();
        }

        sharedFile.Downloads++;
        sharedFile.LastDownloadedUtc = _clock.UtcNow;
        await _session.SaveAsync(sharedFile);

        return File(stream, sharedFile.ContentType ?? "application/octet-stream", sharedFile.FileName);
    }
}
