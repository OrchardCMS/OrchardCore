using System.Globalization;
using System.IO.Compression;
using System.Net.Mime;
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
using OrchardCore.FileStorage;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Json;
using OrchardCore.Localization;
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
    private readonly IWorkflowTypeStore _workflowTypeStore;
    private readonly IWorkflowTypeVersionStore _workflowTypeVersionStore;
    private readonly IWorkflowTypeIdGenerator _workflowTypeIdGenerator;
    private readonly IAuthorizationService _authorizationService;
    private readonly INotifier _notifier;
    private readonly IShapeFactory _shapeFactory;
    private readonly JsonSerializerOptions _documentJsonSerializerOptions;
    private readonly ITempDirectoryProvider _tempDirectoryProvider;
    private readonly IEnumerable<IJSLocalizer> _jsLocalizers;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public WorkflowTypeController
    (
        IOptions<PagerOptions> pagerOptions,
        ISession session,
        IWorkflowTypeStore workflowTypeStore,
        IWorkflowTypeVersionStore workflowTypeVersionStore,
        IWorkflowTypeIdGenerator workflowTypeIdGenerator,
        IAuthorizationService authorizationService,
        IShapeFactory shapeFactory,
        INotifier notifier,
        IStringLocalizer<WorkflowTypeController> stringLocalizer,
        IHtmlLocalizer<WorkflowTypeController> htmlLocalizer,
        ITempDirectoryProvider tempDirectoryProvider,
        IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions,
        IEnumerable<IJSLocalizer> jsLocalizers)
    {
        _pagerOptions = pagerOptions.Value;
        _session = session;
        _workflowTypeStore = workflowTypeStore;
        _workflowTypeVersionStore = workflowTypeVersionStore;
        _workflowTypeIdGenerator = workflowTypeIdGenerator;
        _authorizationService = authorizationService;
        _notifier = notifier;
        _shapeFactory = shapeFactory;
        _tempDirectoryProvider = tempDirectoryProvider;
        S = stringLocalizer;
        H = htmlLocalizer;
        _documentJsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
        _jsLocalizers = jsLocalizers;
    }

    [Admin("Workflows/Types", "WorkflowTypes")]
    public async Task<IActionResult> Index(WorkflowTypeIndexOptions options, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ManageWorkflows))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, _pagerOptions);

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
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ManageWorkflows))
        {
            return Forbid();
        }

        if (itemIds?.Any() == true)
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
                    var deletedWorkflowTypeNames = new List<string>();

                    foreach (var entry in checkedEntries)
                    {
                        var workflowType = await _workflowTypeStore.GetAsync(entry.Id);

                        if (workflowType != null)
                        {
                            await _workflowTypeStore.DeleteAsync(workflowType);
                            deletedWorkflowTypeNames.Add(workflowType.Name);
                        }
                    }

                    if (deletedWorkflowTypeNames.Count > 0)
                    {
                        await _notifier.SuccessAsync(H.Plural(deletedWorkflowTypeNames.Count, "The workflow \"{1}\" has been deleted.", "The following workflows have been deleted: {1}.", string.Join(", ", deletedWorkflowTypeNames)));
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
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ManageWorkflows))
        {
            return Forbid();
        }

        return await ExportWorkflows(id);
    }

    public async Task<IActionResult> Create(string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ManageWorkflows))
        {
            return Forbid();
        }

        // A new workflow records the data of its activities, to see what its runs did; existing ones keep their setting.
        return View(new WorkflowTypePropertiesViewModel
        {
            IsEnabled = true,
            RecordActivityData = true,
            ReturnUrl = returnUrl,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(WorkflowTypePropertiesViewModel viewModel)
    {
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ManageWorkflows))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var workflowType = new WorkflowType();
        workflowType.WorkflowTypeId = _workflowTypeIdGenerator.GenerateUniqueId(workflowType);
        ApplyProperties(workflowType, viewModel);

        await _workflowTypeStore.SaveAsync(workflowType);

        return RedirectToAction(nameof(Edit), new
        {
            workflowType.Id,
        });
    }

    /// <summary>
    /// The properties of a workflow are edited in the designer, on its Workflow tab, so the old properties page
    /// opens the designer, or the page that creates a workflow.
    /// </summary>
    public IActionResult EditProperties(long? id, string returnUrl = null)
        => id is null
            ? RedirectToAction(nameof(Create), new { returnUrl })
            : RedirectToAction(nameof(Edit), new { id });

    public async Task<IActionResult> Clone(long id, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ManageWorkflows))
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
            DeleteFinishedWorkflows = workflowType.DeleteFinishedWorkflows,
            IsActivity = workflowType.IsActivity,
            BranchingMode = workflowType.BranchingMode,
            FaultOnScriptErrors = workflowType.FaultOnScriptErrors,
            RecordActivityData = workflowType.RecordActivityData,
            ReturnUrl = returnUrl,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Clone(WorkflowTypePropertiesViewModel viewModel, long id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ManageWorkflows))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var existingWorkflowType = await _session.GetAsync<WorkflowType>(id);

        if (existingWorkflowType == null)
        {
            return NotFound();
        }

        var workflowType = new WorkflowType();
        workflowType.WorkflowTypeId = _workflowTypeIdGenerator.GenerateUniqueId(workflowType);
        ApplyProperties(workflowType, viewModel);
        workflowType.Activities = existingWorkflowType.Activities;
        workflowType.Transitions = existingWorkflowType.Transitions;
        workflowType.Variables = existingWorkflowType.Variables.Select(variable => variable.Clone()).ToList();

        await _workflowTypeStore.SaveAsync(workflowType);

        return RedirectToAction(nameof(Edit), new
        {
            workflowType.Id,
        });
    }

    private static void ApplyProperties(WorkflowType workflowType, WorkflowTypePropertiesViewModel viewModel)
    {
        workflowType.Name = viewModel.Name?.Trim();
        workflowType.IsEnabled = viewModel.IsEnabled;
        workflowType.IsSingleton = viewModel.IsSingleton;
        workflowType.LockTimeout = viewModel.LockTimeout;
        workflowType.LockExpiration = viewModel.LockExpiration;
        workflowType.DeleteFinishedWorkflows = viewModel.DeleteFinishedWorkflows;
        workflowType.IsActivity = viewModel.IsActivity;
        workflowType.BranchingMode = viewModel.BranchingMode;
        workflowType.FaultOnScriptErrors = viewModel.FaultOnScriptErrors;
        workflowType.RecordActivityData = viewModel.RecordActivityData;
    }

    /// <summary>
    /// The workflow designer. <paramref name="activityId"/> selects an activity and opens its editor, which
    /// keeps the old activity edit URLs working (they redirect here).
    /// </summary>
    public async Task<IActionResult> Edit(long id, string activityId = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ManageWorkflows))
        {
            return Forbid();
        }

        var workflowType = await _workflowTypeStore.GetAsync(id);

        if (workflowType == null)
        {
            return NotFound();
        }

        return View(new WorkflowDesignerViewModel
        {
            WorkflowType = workflowType,
            ConfigJson = WorkflowDesignerConfigBuilder.Build(
                Url,
                User,
                _jsLocalizers,
                workflowType,
                initialActivityId: activityId,
                canRun: await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ExecuteWorkflows)),
        });
    }

    /// <summary>
    /// A version of the workflow type, in the read-only designer.
    /// </summary>
    public async Task<IActionResult> Version(long id, string versionId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ManageWorkflows))
        {
            return Forbid();
        }

        var workflowType = await _workflowTypeStore.GetAsync(id);
        var version = workflowType is null || string.IsNullOrEmpty(versionId) ? null : await _workflowTypeVersionStore.GetAsync(versionId);

        if (version is null || version.WorkflowTypeId != workflowType.WorkflowTypeId)
        {
            return NotFound();
        }

        return View(new WorkflowDesignerViewModel
        {
            WorkflowType = workflowType,
            Version = version,
            ConfigJson = WorkflowDesignerConfigBuilder.BuildForVersion(Url, User, _jsLocalizers, workflowType, versionId),
        });
    }

    /// <summary>
    /// Two definitions of the workflow type side by side: version ids, or <c>draft</c>.
    /// </summary>
    public async Task<IActionResult> CompareVersions(long id, string from, string to)
    {
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ManageWorkflows))
        {
            return Forbid();
        }

        var workflowType = await _workflowTypeStore.GetAsync(id);

        if (workflowType is null || string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
        {
            return NotFound();
        }

        return View(new WorkflowDesignerViewModel
        {
            WorkflowType = workflowType,
            ConfigJson = WorkflowDesignerConfigBuilder.BuildForComparison(Url, User, _jsLocalizers, workflowType, from, to),
        });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, WorkflowsPermissions.ManageWorkflows))
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

    private async Task<IActionResult> ExportWorkflows(params long[] itemIds)
    {
        using var fileBuilder = new TemporaryFileBuilder(_tempDirectoryProvider.GetRootDirectory());
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

        return new PhysicalFileResult(archiveFileName, MediaTypeNames.Application.Zip)
        {
            FileDownloadName = packageName + ".zip",
        };
    }
}
