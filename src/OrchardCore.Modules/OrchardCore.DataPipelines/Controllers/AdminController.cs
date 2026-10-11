using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.DataPipelines.Controllers;

/// <summary>
/// The admin pages of data pipelines: the list, the designer, and the runs.
/// </summary>
public sealed class AdminController : Controller
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    private readonly DataPipelineManager _pipelineManager;
    private readonly DataPipelineRunManager _runManager;
    private readonly DataPipelineSharedFileManager _sharedFileManager;
    private readonly IDataPipelineStepTypeManager _stepTypeManager;
    private readonly OrchardCore.Modules.IClock _clock;
    private readonly IAuthorizationService _authorizationService;
    private readonly IShapeFactory _shapeFactory;
    private readonly ISiteService _siteService;
    private readonly INotifier _notifier;
    private readonly IHtmlLocalizer H;
    private readonly IStringLocalizer S;

    public AdminController(
        DataPipelineManager pipelineManager,
        DataPipelineRunManager runManager,
        DataPipelineSharedFileManager sharedFileManager,
        IDataPipelineStepTypeManager stepTypeManager,
        OrchardCore.Modules.IClock clock,
        IAuthorizationService authorizationService,
        IShapeFactory shapeFactory,
        ISiteService siteService,
        INotifier notifier,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IStringLocalizer<AdminController> stringLocalizer)
    {
        _pipelineManager = pipelineManager;
        _runManager = runManager;
        _sharedFileManager = sharedFileManager;
        _stepTypeManager = stepTypeManager;
        _clock = clock;
        _authorizationService = authorizationService;
        _shapeFactory = shapeFactory;
        _siteService = siteService;
        _notifier = notifier;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    [Admin("DataPipelines", "DataPipelines")]
    public async Task<IActionResult> Index(string q, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DataPipelinePermissions.ViewDataPipelines))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, (await _siteService.GetSiteSettingsAsync()).PageSize);
        var (pipelines, count) = await _pipelineManager.ListAsync(q, pager.GetStartIndex(), pager.PageSize);

        var model = new DataPipelineIndexViewModel
        {
            Search = q,
            TotalCount = count,
            Pager = await _shapeFactory.PagerAsync(pager, count, new RouteData(new RouteValueDictionary { ["q"] = q })),
            CanManage = await _authorizationService.AuthorizeAsync(User, DataPipelinePermissions.ManageDataPipelines),
            CanRun = await _authorizationService.AuthorizeAsync(User, DataPipelinePermissions.RunDataPipelines),
        };

        foreach (var pipeline in pipelines)
        {
            var (runs, _) = await _runManager.ListAsync(pipeline.PipelineId, 0, 1);
            model.Pipelines.Add(new DataPipelineEntryViewModel { Pipeline = pipeline, LastRun = runs.Count > 0 ? runs[0] : null });
        }

        return View(model);
    }

    [Admin("DataPipelines/Create", "DataPipelinesCreate")]
    public async Task<IActionResult> Create()
    {
        if (!await _authorizationService.AuthorizeAsync(User, DataPipelinePermissions.ManageDataPipelines))
        {
            return Forbid();
        }

        return View(new DataPipelineCreateViewModel());
    }

    [HttpPost]
    [ActionName(nameof(Create))]
    [Admin("DataPipelines/Create", "DataPipelinesCreate")]
    public async Task<IActionResult> CreatePost(DataPipelineCreateViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DataPipelinePermissions.ManageDataPipelines))
        {
            return Forbid();
        }

        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), S["The name is required."]);

            return View(model);
        }

        var pipeline = await _pipelineManager.CreateAsync(model.Name, model.Description, User);

        return RedirectToAction(nameof(Edit), new { pipelineId = pipeline.PipelineId });
    }

    [Admin("DataPipelines/{pipelineId}/Edit", "DataPipelinesEdit")]
    public async Task<IActionResult> Edit(string pipelineId, string stepId = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DataPipelinePermissions.ViewDataPipelines))
        {
            return Forbid();
        }

        var pipeline = await _pipelineManager.GetAsync(pipelineId);

        if (pipeline is null)
        {
            return NotFound();
        }

        var canManage = await _authorizationService.AuthorizeAsync(User, DataPipelinePermissions.ManageDataPipelines);

        return View("Designer", new DataPipelineDesignerViewModel
        {
            Pipeline = pipeline,
            ConfigJson = BuildConfig(pipeline.PipelineId, "designer", !canManage, null, stepId),
        });
    }

    [Admin("DataPipelines/{pipelineId}/Version", "DataPipelinesVersion")]
    public async Task<IActionResult> Version(string pipelineId, string versionId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DataPipelinePermissions.ViewDataPipelines))
        {
            return Forbid();
        }

        var pipeline = await _pipelineManager.GetAsync(pipelineId);
        var version = pipeline is null ? null : await _pipelineManager.GetVersionAsync(pipelineId, versionId);

        if (version is null)
        {
            return NotFound();
        }

        return View("Designer", new DataPipelineDesignerViewModel
        {
            Pipeline = pipeline,
            Version = version,
            ConfigJson = BuildConfig(pipeline.PipelineId, "version", readOnly: true, versionId, null),
        });
    }

    [HttpPost]
    [Admin("DataPipelines/{pipelineId}/Delete", "DataPipelinesDelete")]
    public async Task<IActionResult> Delete(string pipelineId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DataPipelinePermissions.ManageDataPipelines))
        {
            return Forbid();
        }

        var pipeline = await _pipelineManager.GetAsync(pipelineId);

        if (pipeline is null)
        {
            return NotFound();
        }

        await _pipelineManager.DeleteAsync(pipeline);
        await _notifier.SuccessAsync(H["The pipeline '{0}' was deleted.", pipeline.Name]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Admin("DataPipelines/{pipelineId}/Run", "DataPipelinesRunNow")]
    public async Task<IActionResult> RunNow(string pipelineId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DataPipelinePermissions.RunDataPipelines))
        {
            return Forbid();
        }

        var pipeline = await _pipelineManager.GetAsync(pipelineId);

        if (pipeline is null)
        {
            return NotFound();
        }

        if (pipeline.Published is null || !pipeline.IsEnabled)
        {
            await _notifier.ErrorAsync(pipeline.Published is null
                ? H["Publish the pipeline '{0}' before running it.", pipeline.Name]
                : H["Enable the pipeline '{0}' before running it.", pipeline.Name]);

            return RedirectToAction(nameof(Index));
        }

        var run = await _runManager.QueueAsync(pipeline, new DataPipelineRunRequest { TriggeredBy = User });
        await _notifier.SuccessAsync(H["The pipeline '{0}' will run in the background.", pipeline.Name]);

        return RedirectToAction(nameof(Run), new { runId = run.RunId });
    }

    [Admin("DataPipelines/Runs/{pipelineId?}", "DataPipelinesRuns")]
    public async Task<IActionResult> Runs(string pipelineId, string q, DataPipelineRunStatus? status, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DataPipelinePermissions.ViewDataPipelines))
        {
            return Forbid();
        }

        var pipeline = string.IsNullOrEmpty(pipelineId) ? null : await _pipelineManager.GetAsync(pipelineId);

        if (!string.IsNullOrEmpty(pipelineId) && pipeline is null)
        {
            return NotFound();
        }

        var pager = new Pager(pagerParameters, (await _siteService.GetSiteSettingsAsync()).PageSize);
        var filter = new DataPipelineRunFilter { PipelineId = pipelineId, Search = pipeline is null ? q : null, Status = status };
        var (runs, count) = await _runManager.ListAsync(filter, pager.GetStartIndex(), pager.PageSize);

        return View(new DataPipelineRunsViewModel
        {
            Pipeline = pipeline,
            Search = filter.Search,
            Status = status,
            TotalCount = count,
            Runs = runs.ToList(),
            Pager = await _shapeFactory.PagerAsync(pager, count, new RouteData(new RouteValueDictionary
            {
                ["pipelineId"] = pipelineId,
                ["q"] = filter.Search,
                ["status"] = status?.ToString(),
            })),
            CanRun = await _authorizationService.AuthorizeAsync(User, DataPipelinePermissions.RunDataPipelines),
        });
    }

    [Admin("DataPipelines/Run/{runId}", "DataPipelinesRun")]
    public async Task<IActionResult> Run(string runId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DataPipelinePermissions.ViewDataPipelines))
        {
            return Forbid();
        }

        var run = await _runManager.GetAsync(runId);

        if (run is null)
        {
            return NotFound();
        }

        // A step is named by its title, or else by the name of its type.
        var stepNames = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var step in run.Definition?.Steps ?? [])
        {
            stepNames[step.StepId] = !string.IsNullOrWhiteSpace(step.Title)
                ? step.Title
                : _stepTypeManager.GetStepType(step.Type)?.DisplayName.Value ?? step.Type;
        }

        return View(new DataPipelineRunViewModel
        {
            Run = run,
            StepNames = stepNames,
            Pipeline = await _pipelineManager.GetAsync(run.PipelineId),
            CanCancel = !run.IsCompleted && await _authorizationService.AuthorizeAsync(User, DataPipelinePermissions.RunDataPipelines),
        });
    }

    [HttpPost]
    [Admin("DataPipelines/Run/{runId}/Cancel", "DataPipelinesCancelRun")]
    public async Task<IActionResult> CancelRun(string runId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DataPipelinePermissions.RunDataPipelines))
        {
            return Forbid();
        }

        var run = await _runManager.GetAsync(runId);

        if (run is null)
        {
            return NotFound();
        }

        if (await _runManager.CancelAsync(run, User))
        {
            await _notifier.SuccessAsync(H["The run is being cancelled."]);
        }

        return RedirectToAction(nameof(Run), new { runId });
    }

    [Admin("DataPipelines/SharedFiles", "DataPipelinesSharedFiles")]
    public async Task<IActionResult> SharedFiles(string q, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DataPipelinePermissions.ManageDataPipelines))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, (await _siteService.GetSiteSettingsAsync()).PageSize);
        var (files, count) = await _sharedFileManager.ListAsync(q, pager.GetStartIndex(), pager.PageSize);

        return View(new DataPipelineSharedFilesViewModel
        {
            Search = q,
            TotalCount = count,
            Files = files.ToList(),
            UtcNow = _clock.UtcNow,
            Pager = await _shapeFactory.PagerAsync(pager, count, new RouteData(new RouteValueDictionary { ["q"] = q })),
        });
    }

    [HttpPost]
    [Admin("DataPipelines/SharedFiles/{fileId}/Revoke", "DataPipelinesRevokeSharedFile")]
    public async Task<IActionResult> RevokeSharedFile(string fileId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DataPipelinePermissions.ManageDataPipelines))
        {
            return Forbid();
        }

        var file = await _sharedFileManager.GetAsync(fileId);

        if (file is null)
        {
            return NotFound();
        }

        file.RevokedUtc ??= _clock.UtcNow;
        await _sharedFileManager.SaveAsync(file);
        await _notifier.SuccessAsync(H["The link to '{0}' no longer works.", file.FileName]);

        return RedirectToAction(nameof(SharedFiles));
    }

    [HttpPost]
    [Admin("DataPipelines/SharedFiles/{fileId}/Delete", "DataPipelinesDeleteSharedFile")]
    public async Task<IActionResult> DeleteSharedFile(string fileId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DataPipelinePermissions.ManageDataPipelines))
        {
            return Forbid();
        }

        var file = await _sharedFileManager.GetAsync(fileId);

        if (file is null)
        {
            return NotFound();
        }

        _sharedFileManager.Delete(file);
        await _notifier.SuccessAsync(H["The shared file '{0}' was deleted.", file.FileName]);

        return RedirectToAction(nameof(SharedFiles));
    }

    private string BuildConfig(string pipelineId, string mode, bool readOnly, string versionId, string stepId)
    {
        var designer = mode == "designer" && !readOnly;
        var query = string.IsNullOrEmpty(versionId) ? null : new { versionId };

        string Endpoint(string action, bool enabled = true)
        {
            if (!enabled)
            {
                return null;
            }

            var url = Url.Action(action, "DataPipelineDesigner", new { area = "OrchardCore.DataPipelines", pipelineId });

            return query is null ? url : $"{url}?versionId={Uri.EscapeDataString(versionId)}";
        }

        var config = new
        {
            pipelineId,
            mode,
            readOnly,
            urls = new
            {
                definition = Endpoint("Definition"),
                library = Endpoint("Library"),
                save = Endpoint("Save", designer),
                addStep = Endpoint("AddStep", designer),
                editor = Endpoint("Editor", designer),
                settings = Endpoint("Settings", designer),
                fields = Endpoint("Fields"),
                preview = Endpoint("Preview", !readOnly),
                publish = Endpoint("Publish", designer),
                discard = Endpoint("Discard", designer),
                versions = Endpoint("Versions"),
                restore = Endpoint("Restore", designer),
                run = Endpoint("Run", mode == "designer"),
                runs = Endpoint("Runs"),
                cancelRun = Endpoint("CancelRun", mode == "designer"),
            },
            listUrl = Url.Action(nameof(Index), "Admin", new { area = "OrchardCore.DataPipelines" }),
            designerUrl = Url.Action(nameof(Edit), "Admin", new { area = "OrchardCore.DataPipelines", pipelineId }),
            versionPageUrl = Url.Action(nameof(Version), "Admin", new { area = "OrchardCore.DataPipelines", pipelineId }),
            runsPageUrl = Url.Action(nameof(Runs), "Admin", new { area = "OrchardCore.DataPipelines", pipelineId }),
            currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            initialStepId = stepId,
            translations = new Dictionary<string, string>(),
        };

        return JsonSerializer.Serialize(config, _jsonOptions);
    }
}
