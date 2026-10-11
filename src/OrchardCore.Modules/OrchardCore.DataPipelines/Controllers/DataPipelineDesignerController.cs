using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataPipelines.ViewModels;
using OrchardCore.DataPipelines.ViewModels.Designer;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.DataPipelines.Controllers;

/// <summary>
/// The JSON endpoints of the pipeline designer. Changes are saved into the pipeline's draft, which runs never use until
/// it is published.
/// </summary>
[Admin("DataPipelines/{pipelineId}/Designer/{action}", "DataPipelineDesigner{action}")]
public sealed class DataPipelineDesignerController : Controller
{
    private const string FragmentViewName = "DesignerFragment";
    private const string SettingsPartialName = "DataPipelineDesignerSettings";

    private readonly DataPipelineManager _pipelineManager;
    private readonly DataPipelineRunManager _runManager;
    private readonly DataPipelineAnalyzer _analyzer;
    private readonly DataPipelineExecutor _executor;
    private readonly IDataPipelineStepTypeManager _stepTypeManager;
    private readonly DataPipelineDesignerModelBuilder _modelBuilder;
    private readonly DataPipelineEditorContext _editorContext;
    private readonly IDisplayManager<DataPipelineStep> _displayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IIdGenerator _idGenerator;
    private readonly IOptions<ShellOptions> _shellOptions;
    private readonly ShellSettings _shellSettings;
    private readonly IOptions<DataPipelineOptions> _options;
    private readonly IStringLocalizer S;

    public DataPipelineDesignerController(
        DataPipelineManager pipelineManager,
        DataPipelineRunManager runManager,
        DataPipelineAnalyzer analyzer,
        DataPipelineExecutor executor,
        IDataPipelineStepTypeManager stepTypeManager,
        DataPipelineDesignerModelBuilder modelBuilder,
        DataPipelineEditorContext editorContext,
        IDisplayManager<DataPipelineStep> displayManager,
        IUpdateModelAccessor updateModelAccessor,
        IAuthorizationService authorizationService,
        IIdGenerator idGenerator,
        IOptions<ShellOptions> shellOptions,
        ShellSettings shellSettings,
        IOptions<DataPipelineOptions> options,
        IStringLocalizer<DataPipelineDesignerController> localizer)
    {
        _pipelineManager = pipelineManager;
        _runManager = runManager;
        _analyzer = analyzer;
        _executor = executor;
        _stepTypeManager = stepTypeManager;
        _modelBuilder = modelBuilder;
        _editorContext = editorContext;
        _displayManager = displayManager;
        _updateModelAccessor = updateModelAccessor;
        _authorizationService = authorizationService;
        _idGenerator = idGenerator;
        _shellOptions = shellOptions;
        _shellSettings = shellSettings;
        _options = options;
        S = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Definition(string pipelineId, string versionId = null)
    {
        if (!await AuthorizeAsync(DataPipelinePermissions.ViewDataPipelines))
        {
            return this.ApiForbidProblem();
        }

        var pipeline = await _pipelineManager.GetAsync(pipelineId);

        if (pipeline is null)
        {
            return this.ApiNotFoundProblem();
        }

        DataPipelineVersion version = null;
        var definition = pipeline.GetEditableDefinition();

        if (!string.IsNullOrEmpty(versionId))
        {
            version = await _pipelineManager.GetVersionAsync(pipelineId, versionId);

            if (version is null)
            {
                return this.ApiNotFoundProblem();
            }

            definition = version.Definition;
        }

        var analysis = await AnalyzeAsync(definition);
        var published = await _pipelineManager.GetVersionAsync(pipelineId, pipeline.PublishedVersionId);
        var (runs, _) = await _runManager.ListAsync(pipelineId, 0, 1);
        var lastRun = runs.Count > 0 ? runs[0] : null;

        return Ok(new DesignerDefinition
        {
            PipelineId = pipeline.PipelineId,
            Revision = pipeline.Revision,
            HasDraft = pipeline.Draft is not null,
            DraftModifiedBy = pipeline.DraftModifiedBy,
            DraftModifiedByUserId = pipeline.DraftModifiedByUserId,
            DraftModifiedUtc = pipeline.DraftModifiedUtc,
            Settings = new DesignerSettings { Name = pipeline.Name, Description = pipeline.Description },
            Nodes = await _modelBuilder.BuildNodesAsync(definition),
            Connections = definition.Connections.Select(DesignerConnection.From).ToList(),
            Issues = analysis.Issues.Select(DesignerIssue.From).ToList(),
            PublishedVersion = DesignerVersion.From(published, pipeline),
            Version = DesignerVersion.From(version, pipeline),
            // The designer offers to run a pipeline published while it's open; the run endpoint rejects a disabled one.
            CanRun = await AuthorizeAsync(DataPipelinePermissions.RunDataPipelines),
            LastRun = DataPipelineDesignerModelBuilder.BuildRun(lastRun, RunUrl(lastRun), await AuthorizeAsync(DataPipelinePermissions.RunDataPipelines)),
        });
    }

    [HttpGet]
    public async Task<IActionResult> Library()
    {
        if (!await AuthorizeAsync(DataPipelinePermissions.ViewDataPipelines))
        {
            return this.ApiForbidProblem();
        }

        return Ok(_modelBuilder.BuildLibrary());
    }

    [HttpPost]
    public async Task<IActionResult> Save(string pipelineId, [FromBody] DesignerSaveRequest request)
    {
        if (!await AuthorizeAsync(DataPipelinePermissions.ManageDataPipelines))
        {
            return this.ApiForbidProblem();
        }

        var pipeline = await _pipelineManager.GetAsync(pipelineId);

        if (pipeline is null || request is null)
        {
            return pipeline is null ? this.ApiNotFoundProblem() : this.ApiBadRequestProblem();
        }

        try
        {
            var revision = await _pipelineManager.SaveDraftAsync(pipeline, request.Revision, draft =>
            {
                // Removed steps are kept for a while, so undoing their removal brings back their settings.
                var removed = new HashSet<string>(request.RemovedStepIds ?? [], StringComparer.Ordinal);
                pipeline.RemovedSteps.AddRange(draft.Steps.Where(step => removed.Contains(step.StepId)));
                draft.Steps.RemoveAll(step => removed.Contains(step.StepId));

                foreach (var stepId in request.RestoredStepIds ?? [])
                {
                    var restored = pipeline.RemovedSteps.LastOrDefault(step => step.StepId == stepId);

                    if (restored is not null && draft.FindStep(stepId) is null)
                    {
                        pipeline.RemovedSteps.Remove(restored);
                        draft.Steps.Add(restored);
                    }
                }

                if (pipeline.RemovedSteps.Count > 50)
                {
                    pipeline.RemovedSteps.RemoveRange(0, pipeline.RemovedSteps.Count - 50);
                }

                foreach (var node in request.Nodes ?? [])
                {
                    if (draft.FindStep(node.Id) is { } step)
                    {
                        step.X = node.X;
                        step.Y = node.Y;
                    }
                }

                var stepIds = new HashSet<string>(draft.Steps.Select(step => step.StepId), StringComparer.Ordinal);

                draft.Connections = (request.Connections ?? [])
                    .Where(connection => connection is not null && stepIds.Contains(connection.SourceStepId) && stepIds.Contains(connection.TargetStepId))
                    .Select(connection => connection.ToConnection())
                    .ToList();
            }, User);

            var analysis = await AnalyzeAsync(pipeline.GetEditableDefinition());

            return Ok(new { revision, issues = analysis.Issues.Select(DesignerIssue.From).ToList() });
        }
        catch (DataPipelineConflictException ex)
        {
            return Conflict(ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddStep(string pipelineId, [FromBody] DesignerAddStepRequest request)
    {
        if (!await AuthorizeAsync(DataPipelinePermissions.ManageDataPipelines))
        {
            return this.ApiForbidProblem();
        }

        var pipeline = await _pipelineManager.GetAsync(pipelineId);
        var stepType = _stepTypeManager.GetStepType(request?.Type);

        if (pipeline is null || stepType is null)
        {
            return pipeline is null ? this.ApiNotFoundProblem() : this.ApiBadRequestProblem();
        }

        var step = new DataPipelineStep
        {
            StepId = _idGenerator.GenerateUniqueId(),
            Type = stepType.Name,
            X = request.X,
            Y = request.Y,
        };

        DataPipelineConnection connection = null;

        try
        {
            var revision = await _pipelineManager.SaveDraftAsync(pipeline, request.Revision, draft =>
            {
                draft.Steps.Add(step);

                if (request.ConnectFrom is { } from && draft.FindStep(from.StepId) is { } source && _stepTypeManager.GetStepType(source.Type) is { } sourceType)
                {
                    var output = sourceType.GetOutputs(source).FirstOrDefault(port => port.Name == from.Port);
                    var input = output is null ? null : stepType.GetInputs(step).FirstOrDefault(port => port.Kind == output.Kind);

                    if (input is not null)
                    {
                        connection = new DataPipelineConnection
                        {
                            SourceStepId = source.StepId,
                            SourcePort = output.Name,
                            TargetStepId = step.StepId,
                            TargetPort = input.Name,
                        };

                        draft.Connections.Add(connection);
                    }
                }
            }, User);

            var analysis = await AnalyzeAsync(pipeline.GetEditableDefinition());

            return Ok(new
            {
                revision,
                node = await _modelBuilder.BuildNodeAsync(step),
                connection = connection is null ? null : DesignerConnection.From(connection),
                issues = analysis.Issues.Select(DesignerIssue.From).ToList(),
            });
        }
        catch (DataPipelineConflictException ex)
        {
            return Conflict(ex);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Editor(string pipelineId, string stepId)
    {
        if (!await AuthorizeAsync(DataPipelinePermissions.ManageDataPipelines))
        {
            return this.ApiForbidProblem();
        }

        var pipeline = await _pipelineManager.GetAsync(pipelineId);
        var step = pipeline?.GetEditableDefinition().FindStep(stepId);

        if (step is null)
        {
            return this.ApiNotFoundProblem();
        }

        await PrepareEditorAsync(pipeline, step);

        var editor = await _displayManager.BuildEditorAsync(step, _updateModelAccessor.ModelUpdater, isNew: false, string.Empty, string.Empty);
        editor.Metadata.Type = "DataPipelineStep_Edit";

        return Fragment(new DesignerFragmentViewModel { Shape = editor, PipelineId = pipelineId, StepId = stepId });
    }

    [HttpPost]
    [ActionName(nameof(Editor))]
    public async Task<IActionResult> EditorPost(string pipelineId, string stepId, int revision)
    {
        if (!await AuthorizeAsync(DataPipelinePermissions.ManageDataPipelines))
        {
            return this.ApiForbidProblem();
        }

        var pipeline = await _pipelineManager.GetAsync(pipelineId);
        var current = pipeline?.GetEditableDefinition().FindStep(stepId);

        if (current is null)
        {
            return this.ApiNotFoundProblem();
        }

        // Edit a copy, so an invalid form doesn't change the draft.
        var step = DataPipelineManager.Clone(new DataPipelineDefinition { Steps = [current] }).Steps[0];

        await PrepareEditorAsync(pipeline, step);

        var editor = await _displayManager.UpdateEditorAsync(step, _updateModelAccessor.ModelUpdater, isNew: false, string.Empty, string.Empty);

        if (!ModelState.IsValid)
        {
            editor.Metadata.Type = "DataPipelineStep_Edit";

            return Fragment(new DesignerFragmentViewModel { Shape = editor, PipelineId = pipelineId, StepId = stepId, Valid = false });
        }

        var removedConnections = new List<DataPipelineConnection>();

        try
        {
            var newRevision = await _pipelineManager.SaveDraftAsync(pipeline, revision, draft =>
            {
                var index = draft.Steps.FindIndex(item => item.StepId == stepId);

                if (index < 0)
                {
                    return;
                }

                draft.Steps[index] = step;

                // The new settings may have removed ports, such as the outputs of a step.
                if (_stepTypeManager.GetStepType(step.Type) is { } stepType)
                {
                    var inputs = stepType.GetInputs(step).Select(port => port.Name).ToHashSet(StringComparer.Ordinal);
                    var outputs = stepType.GetOutputs(step).Select(port => port.Name).ToHashSet(StringComparer.Ordinal);

                    removedConnections.AddRange(draft.Connections.Where(connection =>
                        (connection.TargetStepId == stepId && !inputs.Contains(connection.TargetPort)) ||
                        (connection.SourceStepId == stepId && !outputs.Contains(connection.SourcePort))));

                    draft.Connections.RemoveAll(removedConnections.Contains);
                }
            }, User);

            var analysis = await AnalyzeAsync(pipeline.GetEditableDefinition());

            return Ok(new
            {
                valid = true,
                revision = newRevision,
                node = await _modelBuilder.BuildNodeAsync(step),
                removedConnections = removedConnections.Select(DesignerConnection.From).ToList(),
                issues = analysis.Issues.Select(DesignerIssue.From).ToList(),
                reloadEditor = _editorContext.ReloadEditor,
            });
        }
        catch (DataPipelineConflictException ex)
        {
            return Conflict(ex);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Settings(string pipelineId)
    {
        if (!await AuthorizeAsync(DataPipelinePermissions.ManageDataPipelines))
        {
            return this.ApiForbidProblem();
        }

        var pipeline = await _pipelineManager.GetAsync(pipelineId);

        if (pipeline is null)
        {
            return this.ApiNotFoundProblem();
        }

        return Fragment(new DesignerFragmentViewModel
        {
            PartialName = SettingsPartialName,
            PartialModel = new DataPipelineSettingsViewModel { Name = pipeline.Name, Description = pipeline.Description, IsEnabled = pipeline.IsEnabled },
            PipelineId = pipelineId,
        });
    }

    [HttpPost]
    [ActionName(nameof(Settings))]
    public async Task<IActionResult> SettingsPost(string pipelineId, DataPipelineSettingsViewModel model)
    {
        if (!await AuthorizeAsync(DataPipelinePermissions.ManageDataPipelines))
        {
            return this.ApiForbidProblem();
        }

        var pipeline = await _pipelineManager.GetAsync(pipelineId);

        if (pipeline is null)
        {
            return this.ApiNotFoundProblem();
        }

        if (string.IsNullOrWhiteSpace(model?.Name))
        {
            ModelState.AddModelError(nameof(DataPipelineSettingsViewModel.Name), S["The name is required."]);
        }

        if (!ModelState.IsValid)
        {
            return Fragment(new DesignerFragmentViewModel { PartialName = SettingsPartialName, PartialModel = model, PipelineId = pipelineId, Valid = false });
        }

        pipeline.IsEnabled = model.IsEnabled;
        await _pipelineManager.UpdateSettingsAsync(pipeline, model.Name, model.Description);

        var analysis = await AnalyzeAsync(pipeline.GetEditableDefinition());

        return Ok(new
        {
            valid = true,
            revision = pipeline.Revision,
            settings = new DesignerSettings { Name = pipeline.Name, Description = pipeline.Description },
            issues = analysis.Issues.Select(DesignerIssue.From).ToList(),
        });
    }

    [HttpGet]
    public async Task<IActionResult> Fields(string pipelineId, string stepId, string versionId = null)
    {
        if (!await AuthorizeAsync(DataPipelinePermissions.ViewDataPipelines))
        {
            return this.ApiForbidProblem();
        }

        var pipeline = await _pipelineManager.GetAsync(pipelineId);
        var definition = pipeline is null ? null : await GetDefinitionAsync(pipeline, versionId);

        if (definition?.FindStep(stepId) is null)
        {
            return this.ApiNotFoundProblem();
        }

        var analysis = await AnalyzeAsync(definition);
        var step = analysis.GetStep(stepId);

        return Ok(new DesignerStepFields
        {
            StepId = stepId,
            Inputs = step.InputPorts.Select(port => new DesignerPortFields
            {
                Port = port.Name,
                DisplayName = port.DisplayName,
                Kind = port.Kind.ToString(),
                Fields = step.GetInputFields(port.Name).Select(DesignerField.From).ToList(),
            }).ToList(),
            Outputs = step.OutputPorts.Select(port => new DesignerPortFields
            {
                Port = port.Name,
                DisplayName = port.DisplayName,
                Kind = port.Kind.ToString(),
                Fields = (step.Outputs.TryGetValue(port.Name, out var fields) ? fields : []).Select(DesignerField.From).ToList(),
            }).ToList(),
        });
    }

    [HttpPost]
    public async Task<IActionResult> Preview(string pipelineId, [FromBody] DesignerPreviewRequest request, string versionId = null)
    {
        if (!await AuthorizeAsync(DataPipelinePermissions.ManageDataPipelines))
        {
            return this.ApiForbidProblem();
        }

        var pipeline = await _pipelineManager.GetAsync(pipelineId);
        var definition = pipeline is null ? null : await GetDefinitionAsync(pipeline, versionId);
        var step = definition?.FindStep(request?.StepId);

        if (step is null)
        {
            return this.ApiNotFoundProblem();
        }

        var directory = Path.Combine(_shellOptions.Value.ShellsApplicationDataPath, _shellOptions.Value.ShellsContainerName, _shellSettings.Name, "DataPipelines", "Previews", _idGenerator.GenerateUniqueId());
        Directory.CreateDirectory(directory);

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted);
            timeout.CancelAfter(TimeSpan.FromMinutes(2));

            var run = new DataPipelineRunContext
            {
                RunId = "preview",
                PipelineId = pipeline.PipelineId,
                PipelineName = pipeline.Name,
                User = User,
                Services = HttpContext.RequestServices,
                IsPreview = true,
                PreviewRowLimit = Math.Max(1, _options.Value.PreviewRowLimit),
                TemporaryDirectory = directory,
                CancellationToken = timeout.Token,
                StepScope = (_, work) => ShellScope.UsingChildScopeAsync(scope => work(scope.ServiceProvider)),
            };

            var stopwatch = Stopwatch.StartNew();
            var preview = await _executor.PreviewAsync(definition, step.StepId, run);
            var isDestination = _stepTypeManager.GetStepType(step.Type)?.Category == DataPipelineStepCategory.Destination;

            return Ok(DataPipelineDesignerModelBuilder.BuildPreview(preview, isDestination, stopwatch.Elapsed));
        }
        finally
        {
            try
            {
                Directory.Delete(directory, recursive: true);
            }
            catch (IOException)
            {
            }
        }
    }

    [HttpPost]
    public async Task<IActionResult> Publish(string pipelineId, [FromBody] DesignerRevisionRequest request)
    {
        if (!await AuthorizeAsync(DataPipelinePermissions.ManageDataPipelines))
        {
            return this.ApiForbidProblem();
        }

        var pipeline = await _pipelineManager.GetAsync(pipelineId);

        if (pipeline is null)
        {
            return this.ApiNotFoundProblem();
        }

        if (request is null || request.Revision != pipeline.Revision)
        {
            return Conflict(new DataPipelineConflictException(pipeline));
        }

        var analysis = await AnalyzeAsync(pipeline.GetEditableDefinition());

        if (analysis.HasErrors)
        {
            var problem = ProblemDetailsFactory.CreateProblemDetails(HttpContext, StatusCodes.Status400BadRequest, S["Fix the errors of the pipeline before publishing it."]);
            problem.Extensions["issues"] = analysis.Issues.Select(DesignerIssue.From).ToList();

            return new ObjectResult(problem) { StatusCode = StatusCodes.Status400BadRequest };
        }

        var version = await _pipelineManager.PublishAsync(pipeline, User);

        return Ok(new
        {
            issues = analysis.Issues.Select(DesignerIssue.From).ToList(),
            publishedUtc = pipeline.PublishedUtc,
            version = DesignerVersion.From(version, pipeline),
        });
    }

    [HttpPost]
    public async Task<IActionResult> Discard(string pipelineId)
    {
        if (!await AuthorizeAsync(DataPipelinePermissions.ManageDataPipelines))
        {
            return this.ApiForbidProblem();
        }

        var pipeline = await _pipelineManager.GetAsync(pipelineId);

        if (pipeline is null)
        {
            return this.ApiNotFoundProblem();
        }

        await _pipelineManager.DiscardDraftAsync(pipeline);

        return Ok(new { });
    }

    [HttpGet]
    public async Task<IActionResult> Versions(string pipelineId)
    {
        if (!await AuthorizeAsync(DataPipelinePermissions.ViewDataPipelines))
        {
            return this.ApiForbidProblem();
        }

        var pipeline = await _pipelineManager.GetAsync(pipelineId);

        if (pipeline is null)
        {
            return this.ApiNotFoundProblem();
        }

        var versions = await _pipelineManager.ListVersionsAsync(pipelineId);

        return Ok(new
        {
            versions = versions.Select(version => DesignerVersion.From(version, pipeline)).ToList(),
            draft = pipeline.Draft is null
                ? null
                : new { revision = pipeline.Revision, modifiedUtc = pipeline.DraftModifiedUtc, modifiedBy = pipeline.DraftModifiedBy },
        });
    }

    [HttpPost]
    public async Task<IActionResult> Restore(string pipelineId, [FromBody] DesignerRestoreRequest request)
    {
        if (!await AuthorizeAsync(DataPipelinePermissions.ManageDataPipelines))
        {
            return this.ApiForbidProblem();
        }

        var pipeline = await _pipelineManager.GetAsync(pipelineId);
        var version = pipeline is null ? null : await _pipelineManager.GetVersionAsync(pipelineId, request?.VersionId);

        if (version is null)
        {
            return this.ApiNotFoundProblem();
        }

        try
        {
            var revision = await _pipelineManager.RestoreAsync(pipeline, version, request.Revision, User);
            var analysis = await AnalyzeAsync(pipeline.GetEditableDefinition());

            return Ok(new { revision, issues = analysis.Issues.Select(DesignerIssue.From).ToList() });
        }
        catch (DataPipelineConflictException ex)
        {
            return Conflict(ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Run(string pipelineId)
    {
        if (!await AuthorizeAsync(DataPipelinePermissions.RunDataPipelines))
        {
            return this.ApiForbidProblem();
        }

        var pipeline = await _pipelineManager.GetAsync(pipelineId);

        if (pipeline is null)
        {
            return this.ApiNotFoundProblem();
        }

        if (pipeline.Published is null || !pipeline.IsEnabled)
        {
            return this.ApiBadRequestProblem(detail: pipeline.Published is null
                ? S["Publish the pipeline before running it."]
                : S["Enable the pipeline before running it."]);
        }

        var run = await _runManager.QueueAsync(pipeline, new DataPipelineRunRequest
        {
            Trigger = DataPipelineRunRequest.ManualTrigger,
            TriggeredBy = User,
        });

        return Ok(DataPipelineDesignerModelBuilder.BuildRun(run, RunUrl(run), canCancel: true));
    }

    [HttpGet]
    public async Task<IActionResult> Runs(string pipelineId, string runId = null)
    {
        if (!await AuthorizeAsync(DataPipelinePermissions.ViewDataPipelines))
        {
            return this.ApiForbidProblem();
        }

        var canCancel = await AuthorizeAsync(DataPipelinePermissions.RunDataPipelines);

        if (!string.IsNullOrEmpty(runId))
        {
            var run = await _runManager.GetAsync(runId);

            if (run is null || run.PipelineId != pipelineId)
            {
                return this.ApiNotFoundProblem();
            }

            return Ok(DataPipelineDesignerModelBuilder.BuildRun(run, RunUrl(run), canCancel));
        }

        var (runs, _) = await _runManager.ListAsync(pipelineId, 0, 20);

        return Ok(new { runs = runs.Select(run => DataPipelineDesignerModelBuilder.BuildRun(run, RunUrl(run), canCancel)).ToList() });
    }

    [HttpPost]
    public async Task<IActionResult> CancelRun(string pipelineId, [FromBody] DesignerRunRequest request)
    {
        if (!await AuthorizeAsync(DataPipelinePermissions.RunDataPipelines))
        {
            return this.ApiForbidProblem();
        }

        var run = await _runManager.GetAsync(request?.RunId);

        if (run is null || run.PipelineId != pipelineId)
        {
            return this.ApiNotFoundProblem();
        }

        await _runManager.CancelAsync(run, User);

        return Ok(DataPipelineDesignerModelBuilder.BuildRun(run, RunUrl(run), canCancel: true));
    }

    private async Task<DataPipelineDefinition> GetDefinitionAsync(DataPipeline pipeline, string versionId)
    {
        if (string.IsNullOrEmpty(versionId))
        {
            return pipeline.GetEditableDefinition();
        }

        return (await _pipelineManager.GetVersionAsync(pipeline.PipelineId, versionId))?.Definition;
    }

    private async Task PrepareEditorAsync(DataPipeline pipeline, DataPipelineStep step)
    {
        _editorContext.Pipeline = pipeline;
        _editorContext.Step = step;
        _editorContext.Analysis = await AnalyzeAsync(pipeline.GetEditableDefinition());
    }

    private Task<DataPipelineAnalysis> AnalyzeAsync(DataPipelineDefinition definition)
        => _analyzer.AnalyzeAsync(definition, User, HttpContext.RequestServices, HttpContext.RequestAborted);

    private Task<bool> AuthorizeAsync(OrchardCore.Security.Permissions.Permission permission)
        => _authorizationService.AuthorizeAsync(User, permission);

    private string RunUrl(DataPipelineRun run)
        => run is null ? null : Url.Action(nameof(AdminController.Run), "Admin", new { area = "OrchardCore.DataPipelines", runId = run.RunId });

    private ViewResult Fragment(DesignerFragmentViewModel model)
    {
        var result = View(FragmentViewName, model);
        result.ContentType = "application/json; charset=utf-8";

        return result;
    }

    private ObjectResult Conflict(DataPipelineConflictException exception)
    {
        var problem = ProblemDetailsFactory.CreateProblemDetails(HttpContext, StatusCodes.Status409Conflict, S["The pipeline was changed by someone else."]);
        problem.Extensions["currentRevision"] = exception.CurrentRevision;
        problem.Extensions["modifiedBy"] = exception.ModifiedBy;
        problem.Extensions["modifiedUtc"] = exception.ModifiedUtc;

        return new ObjectResult(problem) { StatusCode = StatusCodes.Status409Conflict };
    }
}
