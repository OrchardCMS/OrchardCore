using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Models;
using OrchardCore.DataOrchestrator.Services;
using OrchardCore.DataOrchestrator.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.DataOrchestrator.Controllers;

[Admin]
public sealed class AdminController : Controller
{
    private const string AdminPath = "DataPipelines";

    private static readonly JsonSerializerOptions _camelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly IEtlPipelineService _pipelineService;
    private readonly IEtlPipelineExecutor _executor;
    private readonly IEtlActivityLibrary _activityLibrary;
    private readonly IEtlActivityDisplayManager _activityDisplayManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUpdateModelAccessor _updateModelAccessor;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        IEtlPipelineService pipelineService,
        IEtlPipelineExecutor executor,
        IEtlActivityLibrary activityLibrary,
        IEtlActivityDisplayManager activityDisplayManager,
        IAuthorizationService authorizationService,
        IUpdateModelAccessor updateModelAccessor,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer)
    {
        _pipelineService = pipelineService;
        _executor = executor;
        _activityLibrary = activityLibrary;
        _activityDisplayManager = activityDisplayManager;
        _authorizationService = authorizationService;
        _updateModelAccessor = updateModelAccessor;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    [Admin(AdminPath, "EtlPipelines")]
    public async Task<IActionResult> Index(string q = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, EtlPermissions.ViewEtlPipelines))
        {
            return Forbid();
        }

        var pipelines = (await _pipelineService.ListAsync())
            .OrderBy(x => x.Name)
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            pipelines = pipelines.Where(x =>
                (!string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(x.Description) && x.Description.Contains(q, StringComparison.OrdinalIgnoreCase)));
        }

        var viewModel = new EtlPipelineListViewModel
        {
            Pipelines = pipelines.ToList(),
            Search = q,
        };

        return View(viewModel);
    }

    [Admin(AdminPath + "/Create", "EtlPipelineCreate")]
    public async Task<IActionResult> Create()
    {
        if (!await _authorizationService.AuthorizeAsync(User, EtlPermissions.ManageEtlPipelines))
        {
            return Forbid();
        }

        var viewModel = new EtlPipelinePropertiesViewModel();

        return View(viewModel);
    }

    [HttpPost]
    [Admin(AdminPath + "/Create", "EtlPipelineCreate")]
    public async Task<IActionResult> Create(EtlPipelinePropertiesViewModel viewModel)
    {
        if (!await _authorizationService.AuthorizeAsync(User, EtlPermissions.ManageEtlPipelines))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var pipeline = new EtlPipelineDefinition
        {
            PipelineId = IdGenerator.GenerateId(),
            Name = viewModel.Name,
            Description = viewModel.Description,
            IsEnabled = viewModel.IsEnabled,
            Schedule = viewModel.Schedule,
        };

        await _pipelineService.SaveAsync(pipeline);

        return RedirectToAction(nameof(Edit), new { id = pipeline.Id });
    }

    [Admin(AdminPath + "/Edit/{id}", "EtlPipelineEdit")]
    public async Task<IActionResult> Edit(long id, string localId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, EtlPermissions.ManageEtlPipelines))
        {
            return Forbid();
        }

        var pipeline = await _pipelineService.GetByDocumentIdAsync(id);
        if (pipeline == null)
        {
            return NotFound();
        }

        var newLocalId = string.IsNullOrWhiteSpace(localId) ? Guid.NewGuid().ToString() : localId;
        var availableActivities = _activityLibrary.ListActivities();

        var activityThumbnailShapes = new List<dynamic>();
        foreach (var activity in availableActivities)
        {
            activityThumbnailShapes.Add(await BuildActivityDisplayAsync(activity, pipeline.Id, newLocalId, "Thumbnail"));
        }

        var activityDesignShapes = new List<dynamic>();
        foreach (var activityRecord in pipeline.Activities)
        {
            var activity = _activityLibrary.InstantiateActivity(activityRecord.Name);
            if (activity != null)
            {
                activity.Properties = activityRecord.Properties?.DeepClone() as JsonObject ?? [];
                activityDesignShapes.Add(await BuildActivityDisplayAsync(activity, activityRecord, pipeline.Id, newLocalId, "Design"));
            }
        }

        var activitiesData = pipeline.Activities.Select(a =>
        {
            var activity = _activityLibrary.GetActivityByName(a.Name);
            return new
            {
                id = a.ActivityId,
                x = a.X,
                y = a.Y,
                name = a.Name,
                isStart = a.IsStart,
                isSource = activity is EtlSourceActivity,
                isTransform = activity is EtlTransformActivity,
                isLoad = activity is EtlLoadActivity,
                outcomes = activity?.GetPossibleOutcomes()
                    .Select(o => new { name = o.Name, displayName = o.DisplayName })
                    .ToArray() ?? [],
            };
        }).ToList();

        var pipelineData = new
        {
            id = pipeline.Id,
            name = pipeline.Name,
            activities = activitiesData,
            transitions = pipeline.Transitions.Select(t => new
            {
                sourceActivityId = t.SourceActivityId,
                destinationActivityId = t.DestinationActivityId,
                sourceOutcomeName = t.SourceOutcomeName,
            }),
        };

        var pipelineJson = JsonSerializer.Serialize(pipelineData, _camelCaseOptions);
        var viewModel = new EtlPipelineEditorViewModel
        {
            Pipeline = pipeline,
            PipelineJson = pipelineJson,
            ActivityThumbnailShapes = activityThumbnailShapes,
            ActivityDesignShapes = activityDesignShapes,
            ActivityCategories = _activityLibrary.ListCategories().ToList(),
            LocalId = newLocalId,
            LoadLocalState = !string.IsNullOrWhiteSpace(localId),
            State = pipelineJson,
        };

        return View(viewModel);
    }

    [HttpPost]
    [Admin(AdminPath + "/Edit/{id}", "EtlPipelineEdit")]
    public async Task<IActionResult> Edit(EtlPipelineEditorUpdateModel model, string submitAction)
    {
        if (!await _authorizationService.AuthorizeAsync(User, EtlPermissions.ManageEtlPipelines))
        {
            return Forbid();
        }

        var pipeline = await _pipelineService.GetByDocumentIdAsync(model.Id);
        if (pipeline == null)
        {
            return NotFound();
        }

        if (!TryParsePipelineState(model.State, out var state, out var activities, out var transitions))
        {
            ModelState.AddModelError(nameof(model.State), S["The pipeline editor state could not be read."]);

            return await Edit(pipeline.Id, null);
        }

        var removedActivityIds = GetRemovedActivityIds(state);

        var activityIds = pipeline.Activities
            .Where(activity => !string.IsNullOrEmpty(activity.ActivityId))
            .Where(activity => !removedActivityIds.Contains(activity.ActivityId))
            .Select(activity => activity.ActivityId)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var activityState in activities)
        {
            if (activityState is not JsonObject activityObject ||
                !TryGetString(activityObject, "id", out var activityId))
            {
                continue;
            }

            if (removedActivityIds.Contains(activityId))
            {
                continue;
            }

            var existing = pipeline.Activities.FirstOrDefault(a => a.ActivityId == activityId);
            if (existing != null)
            {
                existing.X = GetRoundedInt32(activityObject, "x");
                existing.Y = GetRoundedInt32(activityObject, "y");
                existing.IsStart = GetBoolean(activityObject, "isStart");
            }
        }

        pipeline.Transitions.Clear();
        foreach (var transitionState in transitions)
        {
            if (transitionState is not JsonObject transitionObject ||
                !TryGetString(transitionObject, "sourceActivityId", out var sourceActivityId) ||
                !TryGetString(transitionObject, "destinationActivityId", out var destinationActivityId))
            {
                continue;
            }

            if (!activityIds.Contains(sourceActivityId) || !activityIds.Contains(destinationActivityId))
            {
                continue;
            }

            pipeline.Transitions.Add(new EtlTransition
            {
                SourceActivityId = sourceActivityId,
                DestinationActivityId = destinationActivityId,
                SourceOutcomeName = TryGetString(transitionObject, "sourceOutcomeName", out var sourceOutcomeName)
                    ? sourceOutcomeName
                    : "Done",
            });
        }

        if (removedActivityIds.Count > 0)
        {
            foreach (var activity in pipeline.Activities.ToArray())
            {
                if (removedActivityIds.Contains(activity.ActivityId))
                {
                    pipeline.Activities.Remove(activity);
                }
            }
        }

        EnsureStartActivity(pipeline.Activities);

        await _pipelineService.SaveAsync(pipeline);

        if (string.Equals(submitAction, "run", StringComparison.OrdinalIgnoreCase))
        {
            if (!await _authorizationService.AuthorizeAsync(User, EtlPermissions.ExecuteEtlPipelines))
            {
                return Forbid();
            }

            var log = await _executor.ExecuteAsync(pipeline);
            await _pipelineService.SaveLogAsync(log);

            return RedirectToAction(nameof(Logs), new { id = pipeline.Id });
        }

        return RedirectToAction(nameof(Edit), new { id = pipeline.Id });
    }

    [Admin(AdminPath + "/EditProperties/{id}", "EtlPipelineEditProperties")]
    public async Task<IActionResult> EditProperties(long id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, EtlPermissions.ManageEtlPipelines))
        {
            return Forbid();
        }

        var pipeline = await _pipelineService.GetByDocumentIdAsync(id);
        if (pipeline == null)
        {
            return NotFound();
        }

        var viewModel = new EtlPipelinePropertiesViewModel
        {
            Name = pipeline.Name,
            Description = pipeline.Description,
            IsEnabled = pipeline.IsEnabled,
            Schedule = pipeline.Schedule,
        };

        return View(viewModel);
    }

    [HttpPost]
    [Admin(AdminPath + "/EditProperties/{id}", "EtlPipelineEditProperties")]
    public async Task<IActionResult> EditProperties(long id, EtlPipelinePropertiesViewModel viewModel)
    {
        if (!await _authorizationService.AuthorizeAsync(User, EtlPermissions.ManageEtlPipelines))
        {
            return Forbid();
        }

        var pipeline = await _pipelineService.GetByDocumentIdAsync(id);
        if (pipeline == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        pipeline.Name = viewModel.Name;
        pipeline.Description = viewModel.Description;
        pipeline.IsEnabled = viewModel.IsEnabled;
        pipeline.Schedule = viewModel.Schedule;

        await _pipelineService.SaveAsync(pipeline);

        return RedirectToAction(nameof(Edit), new { id = pipeline.Id });
    }

    [HttpPost]
    [Admin(AdminPath + "/Delete/{id}", "EtlPipelineDelete")]
    public async Task<IActionResult> Delete(long id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, EtlPermissions.ManageEtlPipelines))
        {
            return Forbid();
        }

        var pipeline = await _pipelineService.GetByDocumentIdAsync(id);
        if (pipeline == null)
        {
            return NotFound();
        }

        await _pipelineService.DeleteAsync(pipeline.PipelineId);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Admin(AdminPath + "/Execute/{id}", "EtlPipelineExecute")]
    public async Task<IActionResult> Execute(long id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, EtlPermissions.ExecuteEtlPipelines))
        {
            return Forbid();
        }

        var pipeline = await _pipelineService.GetByDocumentIdAsync(id);
        if (pipeline == null)
        {
            return NotFound();
        }

        var log = await _executor.ExecuteAsync(pipeline);
        await _pipelineService.SaveLogAsync(log);

        return RedirectToAction(nameof(Logs), new { id = pipeline.Id });
    }

    [Admin(AdminPath + "/Logs/{id}", "EtlPipelineLogs")]
    public async Task<IActionResult> Logs(long id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, EtlPermissions.ViewEtlPipelines))
        {
            return Forbid();
        }

        var pipeline = await _pipelineService.GetByDocumentIdAsync(id);
        if (pipeline == null)
        {
            return NotFound();
        }

        var logs = await _pipelineService.GetLogsAsync(pipeline.PipelineId);

        var viewModel = new EtlExecutionLogListViewModel
        {
            Pipeline = pipeline,
            Logs = logs.OrderByDescending(l => l.StartedUtc).ToList(),
        };

        return View(viewModel);
    }

    [Admin(AdminPath + "/Activity/Create", "EtlActivityCreate")]
    public async Task<IActionResult> CreateActivity(long pipelineId, string activityName, string returnUrl)
    {
        if (!await _authorizationService.AuthorizeAsync(User, EtlPermissions.ManageEtlPipelines))
        {
            return Forbid();
        }

        var activity = _activityLibrary.InstantiateActivity(activityName);
        if (activity == null)
        {
            return NotFound();
        }

        var updater = _updateModelAccessor.ModelUpdater;
        var shape = await _activityDisplayManager.BuildEditorAsync(activity, updater, isNew: true);
        shape.Metadata.Type = "EtlActivity_Edit";

        var viewModel = new EtlActivityEditorViewModel
        {
            PipelineId = pipelineId,
            ActivityName = activityName,
            Activity = activity,
            Editor = shape,
            ReturnUrl = returnUrl,
        };

        return View(viewModel);
    }

    [HttpPost]
    [Admin(AdminPath + "/Activity/Create", "EtlActivityCreate")]
    public async Task<IActionResult> CreateActivity(EtlActivityEditorPostModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, EtlPermissions.ManageEtlPipelines))
        {
            return Forbid();
        }

        var pipeline = await _pipelineService.GetByDocumentIdAsync(model.PipelineId);
        if (pipeline == null)
        {
            return NotFound();
        }

        var activity = _activityLibrary.InstantiateActivity(model.ActivityName);
        if (activity == null)
        {
            return NotFound();
        }

        var updater = _updateModelAccessor.ModelUpdater;
        var shape = await _activityDisplayManager.UpdateEditorAsync(activity, updater, isNew: true);

        if (!ModelState.IsValid)
        {
            shape.Metadata.Type = "EtlActivity_Edit";
            var viewModel = new EtlActivityEditorViewModel
            {
                PipelineId = model.PipelineId,
                ActivityName = model.ActivityName,
                Activity = activity,
                Editor = shape,
                ReturnUrl = model.ReturnUrl,
            };
            return View(viewModel);
        }

        var activityRecord = new EtlActivityRecord
        {
            ActivityId = Guid.NewGuid().ToString("n"),
            Name = model.ActivityName,
            Properties = activity.Properties,
            X = 100,
            Y = 100 + pipeline.Activities.Count * 120,
            IsStart = pipeline.Activities.Count == 0,
        };

        pipeline.Activities.Add(activityRecord);
        await _pipelineService.SaveAsync(pipeline);

        if (Url.IsLocalUrl(model.ReturnUrl))
        {
            return this.Redirect(model.ReturnUrl, true);
        }

        return RedirectToAction(nameof(Edit), new { id = pipeline.Id });
    }

    [Admin(AdminPath + "/Activity/Edit", "EtlActivityEdit")]
    public async Task<IActionResult> EditActivity(long pipelineId, string activityId, string returnUrl)
    {
        if (!await _authorizationService.AuthorizeAsync(User, EtlPermissions.ManageEtlPipelines))
        {
            return Forbid();
        }

        var pipeline = await _pipelineService.GetByDocumentIdAsync(pipelineId);
        if (pipeline == null)
        {
            return NotFound();
        }

        var activityRecord = pipeline.Activities.FirstOrDefault(a => a.ActivityId == activityId);
        if (activityRecord == null)
        {
            return NotFound();
        }

        var activity = _activityLibrary.InstantiateActivity(activityRecord.Name);
        if (activity == null)
        {
            return NotFound();
        }

        activity.Properties = activityRecord.Properties?.DeepClone() as JsonObject ?? [];
        var updater = _updateModelAccessor.ModelUpdater;
        var shape = await _activityDisplayManager.BuildEditorAsync(activity, updater, isNew: false);
        shape.Metadata.Type = "EtlActivity_Edit";

        var viewModel = new EtlActivityEditorViewModel
        {
            PipelineId = pipelineId,
            ActivityId = activityId,
            ActivityName = activityRecord.Name,
            Activity = activity,
            Editor = shape,
            ReturnUrl = returnUrl,
        };

        return View("CreateActivity", viewModel);
    }

    [HttpPost]
    [Admin(AdminPath + "/Activity/Edit", "EtlActivityEdit")]
    public async Task<IActionResult> EditActivity(EtlActivityEditorPostModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, EtlPermissions.ManageEtlPipelines))
        {
            return Forbid();
        }

        var pipeline = await _pipelineService.GetByDocumentIdAsync(model.PipelineId);
        if (pipeline == null)
        {
            return NotFound();
        }

        var activityRecord = pipeline.Activities.FirstOrDefault(a => a.ActivityId == model.ActivityId);
        if (activityRecord == null)
        {
            return NotFound();
        }

        var activity = _activityLibrary.InstantiateActivity(activityRecord.Name);
        if (activity == null)
        {
            return NotFound();
        }

        activity.Properties = activityRecord.Properties?.DeepClone() as JsonObject ?? [];
        var updater = _updateModelAccessor.ModelUpdater;
        var shape = await _activityDisplayManager.UpdateEditorAsync(activity, updater, isNew: false);

        if (!ModelState.IsValid)
        {
            shape.Metadata.Type = "EtlActivity_Edit";
            var viewModel = new EtlActivityEditorViewModel
            {
                PipelineId = model.PipelineId,
                ActivityId = model.ActivityId,
                ActivityName = activityRecord.Name,
                Activity = activity,
                Editor = shape,
                ReturnUrl = model.ReturnUrl,
            };
            return View("CreateActivity", viewModel);
        }

        activityRecord.Properties = activity.Properties;
        await _pipelineService.SaveAsync(pipeline);

        if (Url.IsLocalUrl(model.ReturnUrl))
        {
            return this.Redirect(model.ReturnUrl, true);
        }

        return RedirectToAction(nameof(Edit), new { id = pipeline.Id });
    }

    private static bool TryParsePipelineState(
        string editorState,
        out JsonObject state,
        out JsonArray activities,
        out JsonArray transitions)
    {
        state = null;
        activities = null;
        transitions = null;

        if (string.IsNullOrWhiteSpace(editorState))
        {
            return false;
        }

        try
        {
            state = JsonNode.Parse(editorState) as JsonObject;
        }
        catch (JsonException)
        {
            return false;
        }

        if (state == null)
        {
            return false;
        }

        activities = state["activities"] as JsonArray;
        transitions = state["transitions"] as JsonArray;

        return activities != null && transitions != null;
    }

    private static HashSet<string> GetRemovedActivityIds(JsonObject state)
    {
        var removedActivityIds = new HashSet<string>(StringComparer.Ordinal);

        if (state["removedActivities"] is not JsonArray removedActivities)
        {
            return removedActivityIds;
        }

        foreach (var removedActivity in removedActivities)
        {
            if (removedActivity is JsonValue value &&
                value.TryGetValue<string>(out var activityId) &&
                !string.IsNullOrEmpty(activityId))
            {
                removedActivityIds.Add(activityId);
            }
        }

        return removedActivityIds;
    }

    private static void EnsureStartActivity(IList<EtlActivityRecord> activities)
    {
        if (activities.Count == 0 || activities.Any(activity => activity.IsStart))
        {
            return;
        }

        activities[0].IsStart = true;
    }

    private static bool TryGetString(JsonObject obj, string propertyName, out string value)
    {
        value = null;

        if (obj[propertyName] is not JsonValue node ||
            !node.TryGetValue<string>(out value))
        {
            return false;
        }

        return !string.IsNullOrEmpty(value);
    }

    private static int GetRoundedInt32(JsonObject obj, string propertyName)
    {
        if (obj[propertyName] is not JsonValue node ||
            !node.TryGetValue<double>(out var value))
        {
            return 0;
        }

        return (int)Math.Round(Convert.ToDecimal(value));
    }

    private static bool GetBoolean(JsonObject obj, string propertyName)
    {
        return obj[propertyName] is JsonValue node &&
            node.TryGetValue<bool>(out var value) &&
            value;
    }

    private async Task<dynamic> BuildActivityDisplayAsync(IEtlActivity activity, long pipelineId, string localId, string displayType)
    {
        var activityShape = await _activityDisplayManager.BuildDisplayAsync(activity, _updateModelAccessor.ModelUpdater, displayType);
        activityShape.Metadata.Type = $"EtlActivity_{displayType}";
        activityShape.Properties["Activity"] = activity;
        activityShape.Properties["PipelineId"] = pipelineId;
        activityShape.Properties["ReturnUrl"] = Url.Action(nameof(Edit), new
        {
            id = pipelineId,
            localId,
        });

        return activityShape;
    }

    private async Task<dynamic> BuildActivityDisplayAsync(IEtlActivity activity, EtlActivityRecord activityRecord, long pipelineId, string localId, string displayType)
    {
        var activityShape = await _activityDisplayManager.BuildDisplayAsync(activity, _updateModelAccessor.ModelUpdater, displayType);
        activityShape.Metadata.Type = $"EtlActivity_{displayType}";
        activityShape.Properties["Activity"] = activity;
        activityShape.Properties["ActivityRecord"] = activityRecord;
        activityShape.Properties["PipelineId"] = pipelineId;
        activityShape.Properties["ReturnUrl"] = Url.Action(nameof(Edit), new
        {
            id = pipelineId,
            localId,
        });

        return activityShape;
    }
}
