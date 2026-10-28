using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// Builds the editor of an activity, then updates the activity from the values the editor shows, as if they
/// were posted back unchanged: the errors the drivers report are the errors of the activity's settings.
/// </summary>
public sealed class WorkflowActivityEditorValidator : IWorkflowActivityEditorValidator
{
    private readonly IActivityLibrary _activityLibrary;
    private readonly IWorkflowManager _workflowManager;
    private readonly IActivityDisplayManager _activityDisplayManager;
    private readonly IObjectModelValidator _objectModelValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger;

    public WorkflowActivityEditorValidator(
        IActivityLibrary activityLibrary,
        IWorkflowManager workflowManager,
        IActivityDisplayManager activityDisplayManager,
        IObjectModelValidator objectModelValidator,
        IHttpContextAccessor httpContextAccessor,
        ILogger<WorkflowActivityEditorValidator> logger)
    {
        _activityLibrary = activityLibrary;
        _workflowManager = workflowManager;
        _activityDisplayManager = activityDisplayManager;
        _objectModelValidator = objectModelValidator;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ValidateAsync(ActivityRecord activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        var httpContext = _httpContextAccessor.HttpContext;

        // Editors are built for a request; an activity whose type isn't registered has no editor.
        if (httpContext is null || _activityLibrary.GetActivityByName(activity.Name) is null)
        {
            return [];
        }

        var copy = activity.Clone();
        var activityContext = await _workflowManager.CreateActivityExecutionContextAsync(copy, copy.Properties);
        var actionContext = new ActionContext(httpContext, httpContext.GetRouteData() ?? new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var updater = new EditorReplayUpdater(actionContext, _objectModelValidator);

        try
        {
            var editor = await _activityDisplayManager.BuildEditorAsync(activityContext.Activity, updater, isNew: false, "", "");
            updater.Capture(editor);
            await _activityDisplayManager.UpdateEditorAsync(activityContext.Activity, updater, isNew: false, "", "");
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            _logger.LogWarning(ex, "The settings of the activity '{ActivityId}' ({ActivityName}) couldn't be checked with its editor.", activity.ActivityId, activity.Name);

            return [];
        }

        // A driver read a model its editor doesn't show, so its errors would be about values that were never set.
        if (updater.HasUnknownModel)
        {
            return [];
        }

        return updater.ModelState.Values
            .SelectMany(entry => entry.Errors)
            .Select(error => error.ErrorMessage)
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// An updater that binds the models of the editor that was built, instead of posted values.
    /// </summary>
    private sealed class EditorReplayUpdater : IUpdateModel
    {
        private readonly ActionContext _actionContext;
        private readonly IObjectModelValidator _objectModelValidator;
        private readonly List<object> _models = [];

        public EditorReplayUpdater(ActionContext actionContext, IObjectModelValidator objectModelValidator)
        {
            _actionContext = actionContext;
            _objectModelValidator = objectModelValidator;
        }

        public ModelStateDictionary ModelState => _actionContext.ModelState;

        public bool HasUnknownModel { get; private set; }

        /// <summary>
        /// Keeps the models of the shapes of the editor: the view models the drivers filled in.
        /// </summary>
        public void Capture(IShape editor)
        {
            var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
            var pending = new Stack<object>();
            pending.Push(editor);

            while (pending.TryPop(out var item))
            {
                if (item is not IShape shape || item is ZoneOnDemand || !visited.Add(item))
                {
                    continue;
                }

                _models.Add(shape);

                var type = shape.GetType();

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ShapeViewModel<>) &&
                    type.GetProperty(nameof(ShapeViewModel<object>.Value))?.GetValue(shape) is { } value)
                {
                    _models.Add(value);
                }

                foreach (var child in shape.Items)
                {
                    pending.Push(child);
                }

                foreach (var property in shape.Properties.Values)
                {
                    pending.Push(property);
                }
            }
        }

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model)
            where TModel : class
            => TryUpdateModelAsync(model, string.Empty);

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix)
            where TModel : class
        {
            var source = _models.FirstOrDefault(candidate => candidate is TModel && !ReferenceEquals(candidate, model));

            if (source is null)
            {
                HasUnknownModel = true;

                return Task.FromResult(false);
            }

            foreach (var property in typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.CanRead && property.CanWrite && property.GetIndexParameters().Length == 0)
                {
                    property.SetValue(model, property.GetValue(source));
                }
            }

            return Task.FromResult(TryValidateModel(model, prefix));
        }

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions)
            where TModel : class
            => TryUpdateModelAsync(model, prefix);

        public bool TryValidateModel(object model)
            => TryValidateModel(model, string.Empty);

        public bool TryValidateModel(object model, string prefix)
        {
            _objectModelValidator.Validate(_actionContext, validationState: null, prefix ?? string.Empty, model);

            return ModelState.IsValid;
        }
    }
}
