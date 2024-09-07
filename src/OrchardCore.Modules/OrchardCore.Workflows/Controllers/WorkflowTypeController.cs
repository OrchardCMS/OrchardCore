using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Core.Services;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Json;
using OrchardCore.Navigation;
using OrchardCore.Recipes.Models;
using OrchardCore.Routing;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Deployment;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.ViewModels;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Workflows.Controllers;

[Admin("Workflows/Types/{action}/{id?}", "WorkflowTypes{action}")]
public sealed class WorkflowTypeController : Controller
{
    private readonly PagerOptions _pagerOptions;
    private readonly ISession _session;
    private readonly IActivityLibrary _activityLibrary;
    private readonly IWorkflowManager _workflowManager;
    private readonly IWorkflowTypeStore _workflowTypeStore;
    private readonly IWorkflowTypeIdGenerator _workflowTypeIdGenerator;
    private readonly IAuthorizationService _authorizationService;
    private readonly IActivityDisplayManager _activityDisplayManager;
    private readonly INotifier _notifier;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IShapeFactory _shapeFactory;
    private readonly JsonSerializerOptions _documentJsonSerializerOptions;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public WorkflowTypeController
    (
        IOptions<PagerOptions> pagerOptions,
        ISession session,
        IActivityLibrary activityLibrary,
        IWorkflowManager workflowManager,
        IWorkflowTypeStore workflowTypeStore,
        IWorkflowTypeIdGenerator workflowTypeIdGenerator,
        IAuthorizationService authorizationService,
        IActivityDisplayManager activityDisplayManager,
        IShapeFactory shapeFactory,
        INotifier notifier,
        IStringLocalizer<WorkflowTypeController> stringLocalizer,
        IHtmlLocalizer<WorkflowTypeController> htmlLocalizer,
        IUpdateModelAccessor updateModelAccessor,
        IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions)
    {
        _pagerOptions = pagerOptions.Value;
        _session = session;
        _activityLibrary = activityLibrary;
        _workflowManager = workflowManager;
        _workflowTypeStore = workflowTypeStore;
        _workflowTypeIdGenerator = workflowTypeIdGenerator;
        _authorizationService = authorizationService;
        _activityDisplayManager = activityDisplayManager;
        _notifier = notifier;
        _updateModelAccessor = updateModelAccessor;
        _shapeFactory = shapeFactory;
        S = stringLocalizer;
        H = htmlLocalizer;
        _documentJsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
    }

    [Admin("Workflows/Types", "WorkflowTypes")]
    public async Task<IActionResult> Index(WorkflowTypeIndexOptions options, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());

        options ??= new WorkflowTypeIndexOptions();

        var query = _session.Query<WorkflowType, WorkflowTypeIndex>();

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            query = query.Where(x => x.Name.Contains(options.Search));
        }

        switch (options.Order)
        {
            case WorkflowTypeOrder.Name:
                query = query.OrderBy(u => u.Name);
                break;
        }

        var count = await query.CountAsync();

        var workflowTypes = await query
            .Skip(pager.GetStartIndex())
            .Take(pager.PageSize)
            .ListAsync();

        var dialect = _session.Store.Configuration.SqlDialect;
        var sqlBuilder = dialect.CreateBuilder(_session.Store.Configuration.TablePrefix);
        sqlBuilder.Select();
        sqlBuilder.Distinct();
        sqlBuilder.Selector(nameof(WorkflowIndex), nameof(WorkflowIndex.WorkflowTypeId), _session.Store.Configuration.Schema);
        sqlBuilder.Table(nameof(WorkflowIndex), alias: null, _session.Store.Configuration.Schema);

        // Use existing session connection. Do not use 'using' or dispose the connection.
        var connection = await _session.CreateConnectionAsync();
        var workflowTypeIdsWithInstances = (await connection.QueryAsync<string>(sqlBuilder.ToSqlString(), param: null, _session.CurrentTransaction)).ToList();

        // Maintain previous route data when generating page links.
        var routeData = new RouteData();
        routeData.Values.Add("Options.Filter", options.Filter);
        routeData.Values.Add("Options.Order", options.Order);
        if (!string.IsNullOrEmpty(options.Search))
        {
            routeData.Values.TryAdd("Options.Search", options.Search);
        }

        var pagerShape = await _shapeFactory.PagerAsync(pager, count, routeData);
        var model = new WorkflowTypeIndexViewModel
        {
            WorkflowTypes = workflowTypes
                .Select(x => new WorkflowTypeEntry
                {
                    WorkflowType = x,
                    Id = x.Id,
                    HasInstances = workflowTypeIdsWithInstances.Contains(x.WorkflowTypeId),
                    Name = x.Name,
                })
                .ToList(),
            Options = options,
            Pager = pagerShape,
        };

        model.Options.WorkflowTypesBulkAction =
        [
            new SelectListItem(S["Delete"], nameof(WorkflowTypeBulkAction.Delete)),
        ];

        return View(model);
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    public ActionResult IndexFilterPOST(WorkflowTypeIndexViewModel model)
        => RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { "Options.Search", model.Options.Search },
        });

    [HttpPost]
    [ActionName(nameof(Index))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<IActionResult> BulkEdit(WorkflowTypeIndexOptions options, IEnumerable<long> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
        {
            return Forbid();
        }

        if (itemIds?.Count() > 0)
        {
            var checkedEntries = await _session.Query<WorkflowType, WorkflowTypeIndex>()
                .Where(x => x.DocumentId.IsIn(itemIds)).ListAsync();
            switch (options.BulkAction)
            {
                case WorkflowTypeBulkAction.None:
                    break;
                case WorkflowTypeBulkAction.Export:
                    return await ExportWorkflows(itemIds.ToArray());

                case WorkflowTypeBulkAction.Delete:
                    foreach (var entry in checkedEntries)
                    {
                        var workflowType = await _workflowTypeStore.GetAsync(entry.Id);

                        if (workflowType != null)
                        {
                            await _workflowTypeStore.DeleteAsync(workflowType);
                            await _notifier.SuccessAsync(H["Workflow {0} has been deleted.", workflowType.Name]);
                        }
                    }
                    break;

                default:
                    return BadRequest();
            }
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Export(int id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
        {
            return Forbid();
        }

        return await ExportWorkflows(id);
    }

    public async Task<IActionResult> EditProperties(int? id, string returnUrl = null)

    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
        {
            return Forbid();
        }

        if (id == null)
        {
            return View(new WorkflowTypePropertiesViewModel
            {
                IsEnabled = true,
                ReturnUrl = returnUrl,
            });
        }
        else
        {
            var workflowType = await _session.GetAsync<WorkflowType>(id.Value);

            return View(new WorkflowTypePropertiesViewModel
            {
                Id = workflowType.Id,
                Name = workflowType.Name,
                IsEnabled = workflowType.IsEnabled,
                IsSingleton = workflowType.IsSingleton,
                LockTimeout = workflowType.LockTimeout,
                LockExpiration = workflowType.LockExpiration,
                DeleteFinishedWorkflows = workflowType.DeleteFinishedWorkflows,
                ReturnUrl = returnUrl,
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> EditProperties(WorkflowTypePropertiesViewModel viewModel, long? id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var isNew = id == null;
        var workflowType = default(WorkflowType);

        if (isNew)
        {
            workflowType = new WorkflowType();
            workflowType.WorkflowTypeId = _workflowTypeIdGenerator.GenerateUniqueId(workflowType);
        }
        else
        {
            workflowType = await _session.GetAsync<WorkflowType>(id.Value);

            if (workflowType == null)
            {
                return NotFound();
            }
        }

        workflowType.Name = viewModel.Name?.Trim();
        workflowType.IsEnabled = viewModel.IsEnabled;
        workflowType.IsSingleton = viewModel.IsSingleton;
        workflowType.LockTimeout = viewModel.LockTimeout;
        workflowType.LockExpiration = viewModel.LockExpiration;
        workflowType.DeleteFinishedWorkflows = viewModel.DeleteFinishedWorkflows;

        await _workflowTypeStore.SaveAsync(workflowType);

        return isNew
            ? RedirectToAction(nameof(Edit), new
            {
                workflowType.Id,
            })
            : Url.IsLocalUrl(viewModel.ReturnUrl)
                ? (IActionResult)this.Redirect(viewModel.ReturnUrl, true)
                : RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Duplicate(long id, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
        {
            return Forbid();
        }

        var workflowType = await _session.GetAsync<WorkflowType>(id);

        if (workflowType == null)
        {
            return NotFound();
        }

        return View(new WorkflowTypePropertiesViewModel
        {
            Id = id,
            IsSingleton = workflowType.IsSingleton,
            LockTimeout = workflowType.LockTimeout,
            LockExpiration = workflowType.LockExpiration,
            Name = "Copy-" + workflowType.Name,
            IsEnabled = workflowType.IsEnabled,
            ReturnUrl = returnUrl,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Duplicate(WorkflowTypePropertiesViewModel viewModel, long id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var existingWorkflowType = await _session.GetAsync<WorkflowType>(id);
        var workflowType = new WorkflowType();
        workflowType.WorkflowTypeId = _workflowTypeIdGenerator.GenerateUniqueId(workflowType);

        workflowType.Name = viewModel.Name?.Trim();
        workflowType.IsEnabled = viewModel.IsEnabled;
        workflowType.IsSingleton = viewModel.IsSingleton;
        workflowType.LockTimeout = viewModel.LockTimeout;
        workflowType.LockExpiration = viewModel.LockExpiration;
        workflowType.DeleteFinishedWorkflows = viewModel.DeleteFinishedWorkflows;
        workflowType.Activities = existingWorkflowType.Activities;
        workflowType.Transitions = existingWorkflowType.Transitions;

        await _workflowTypeStore.SaveAsync(workflowType);

        return RedirectToAction(nameof(Edit), new
        {
            workflowType.Id,
        });
    }

    public async Task<IActionResult> Edit(long id, string localId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
        {
            return Forbid();
        }

        var newLocalId = string.IsNullOrWhiteSpace(localId) ? Guid.NewGuid().ToString() : localId;
        var availableActivities = _activityLibrary.ListActivities();
        var workflowType = await _session.GetAsync<WorkflowType>(id);

        if (workflowType == null)
        {
            return NotFound();
        }

        var workflow = _workflowManager.NewWorkflow(workflowType);
        var workflowContext = await _workflowManager.CreateWorkflowExecutionContextAsync(workflowType, workflow);
        var activityContexts = await Task.WhenAll(workflowType.Activities.Select(x =>
            _workflowManager.CreateActivityExecutionContextAsync(x, x.Properties)));
        var workflowCount = await _session
            .QueryIndex<WorkflowIndex>(x => x.WorkflowTypeId == workflowType.WorkflowTypeId).CountAsync();

        var activityThumbnailShapes = new List<dynamic>();
        var index = 0;

        foreach (var activity in availableActivities)
        {
            activityThumbnailShapes.Add(await BuildActivityDisplay(activity, index++, id, newLocalId, "Thumbnail"));
        }

        var activityDesignShapes = new List<dynamic>();
        index = 0;

        foreach (var activityContext in activityContexts)
        {
            activityDesignShapes.Add(await BuildActivityDisplay(activityContext, index++, id, newLocalId, "Design"));
        }

        var activitiesDataQuery = activityContexts.Select(x => new
        {
            Id = x.ActivityRecord.ActivityId,
            x.ActivityRecord.X,
            x.ActivityRecord.Y,
            x.ActivityRecord.Name,
            x.ActivityRecord.IsStart,
            IsEvent = x.Activity.IsEvent(),
            Outcomes = x.Activity.GetPossibleOutcomes(workflowContext, x).ToArray(),
        });

        var workflowTypeData = new
        {
            workflowType.Id,
            workflowType.Name,
            workflowType.IsEnabled,
            Activities = activitiesDataQuery.ToArray(),
            workflowType.Transitions,
        };

        var viewModel = new WorkflowTypeViewModel
        {
            WorkflowType = workflowType,
            WorkflowTypeJson = JConvert.SerializeObject(workflowTypeData, JOptions.CamelCase),
            ActivityThumbnailShapes = activityThumbnailShapes,
            ActivityDesignShapes = activityDesignShapes,
            ActivityCategories = _activityLibrary.ListCategories().ToList(),
            LocalId = newLocalId,
            LoadLocalState = !string.IsNullOrWhiteSpace(localId),
            WorkflowCount = workflowCount,
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(WorkflowTypeUpdateModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
        {
            return Forbid();
        }

        var workflowType = await _workflowTypeStore.GetAsync(model.Id);
        var state = JObject.Parse(model.State);
        var currentActivities = workflowType.Activities.ToDictionary(x => x.ActivityId);
        var activities = state["activities"] as JsonArray;

        var postedActivities = JArray.FromObject(activities).ToDictionary(x => x["id"]?.ToString());
        var removedActivityIdsQuery =
            from activityId in currentActivities.Keys
            where !postedActivities.ContainsKey(activityId)
            select activityId;
        var removedActivityIds = removedActivityIdsQuery.ToList();

        // Remove any orphans (activities deleted on the client).
        foreach (var activityId in removedActivityIds)
        {
            var activityToRemove = currentActivities[activityId];
            workflowType.Activities.Remove(activityToRemove);
            currentActivities.Remove(activityId);
        }

        // Update activities.
        foreach (var activityState in activities)
        {
            var activity = currentActivities[activityState["id"].ToString()];
            activity.X = (int)Math.Round(Convert.ToDecimal(activityState["x"].ToString(), CultureInfo.InvariantCulture), 0);
            activity.Y = (int)Math.Round(Convert.ToDecimal(activityState["y"].ToString(), CultureInfo.InvariantCulture), 0);
            activity.IsStart = Convert.ToBoolean(activityState["isStart"].ToString());
        }

        // Update transitions.
        workflowType.Transitions.Clear();

        var transitions = state["transitions"] as JsonArray;

        foreach (var transitionState in transitions)
        {
            workflowType.Transitions.Add(new Transition
            {
                SourceActivityId = transitionState["sourceActivityId"]?.ToString(),
                DestinationActivityId = transitionState["destinationActivityId"]?.ToString(),
                SourceOutcomeName = transitionState["sourceOutcomeName"]?.ToString(),
            });
        }

        await _workflowTypeStore.SaveAsync(workflowType);
        await _notifier.SuccessAsync(H["Workflow has been saved."]);

        return RedirectToAction(nameof(Edit), new
        {
            id = model.Id,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageWorkflows))
        {
            return Forbid();
        }

        var workflowType = await _workflowTypeStore.GetAsync(id);

        if (workflowType == null)
        {
            return NotFound();
        }

        await _workflowTypeStore.DeleteAsync(workflowType);
        await _notifier.SuccessAsync(H["Workflow {0} deleted", workflowType.Name]);

        return RedirectToAction(nameof(Index));
    }

    private async Task<dynamic> BuildActivityDisplay(IActivity activity, int index, long workflowTypeId,
        string localId, string displayType)
    {
        var activityShape = await _activityDisplayManager.BuildDisplayAsync(activity, _updateModelAccessor.ModelUpdater, displayType);
        activityShape.Metadata.Type = $"Activity_{displayType}";
        activityShape.Properties["Activity"] = activity;
        activityShape.Properties["WorkflowTypeId"] = workflowTypeId;
        activityShape.Properties["Index"] = index;
        activityShape.Properties["ReturnUrl"] = Url.Action(nameof(Edit), new
        {
            id = workflowTypeId,
            localId,
        });

        return activityShape;
    }

    private async Task<dynamic> BuildActivityDisplay(ActivityContext activityContext, int index, long workflowTypeId,
        string localId, string displayType)
    {
        var activityShape = await _activityDisplayManager.BuildDisplayAsync(activityContext.Activity, _updateModelAccessor.ModelUpdater, displayType);
        activityShape.Metadata.Type = $"Activity_{displayType}";
        activityShape.Properties["Activity"] = activityContext.Activity;
        activityShape.Properties["ActivityRecord"] = activityContext.ActivityRecord;
        activityShape.Properties["WorkflowTypeId"] = workflowTypeId;
        activityShape.Properties["Index"] = index;
        activityShape.Properties["ReturnUrl"] = Url.Action(nameof(Edit), new
        {
            id = workflowTypeId,
            localId,
        });

        return activityShape;
    }

    private async Task<IActionResult> ExportWorkflows(params long[] itemIds)
    {
        using var fileBuilder = new TemporaryFileBuilder();
        var archiveFileName = fileBuilder.Folder + ".zip";
        var recipeDescriptor = new RecipeDescriptor();
        var deploymentPlanResult = new DeploymentPlanResult(fileBuilder, recipeDescriptor);
        var workflowTypes = await _workflowTypeStore.GetAsync(itemIds);

        AllWorkflowTypeDeploymentSource.ProcessWorkflowType(deploymentPlanResult, workflowTypes, _documentJsonSerializerOptions);

        await deploymentPlanResult.FinalizeAsync();
        ZipFile.CreateFromDirectory(fileBuilder.Folder, archiveFileName);

        var packageName = itemIds.Length == 1
            ? workflowTypes.FirstOrDefault().Name
            : S["Workflow Types"];

        return new PhysicalFileResult(archiveFileName, "application/zip")
        {
            FileDownloadName = packageName + ".zip",
        };
    }
}
