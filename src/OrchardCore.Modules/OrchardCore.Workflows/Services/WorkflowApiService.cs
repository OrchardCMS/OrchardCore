using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Json;
using OrchardCore.RemoteManagement;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Services;

internal sealed class WorkflowApiService
{
    private readonly IWorkflowTypeStore _workflowTypeStore;
    private readonly IWorkflowStore _workflowStore;
    private readonly IWorkflowManager _workflowManager;
    private readonly IActivityLibrary _activityLibrary;
    private readonly IWorkflowTypeIdGenerator _workflowTypeIdGenerator;
    private readonly IActivityIdGenerator _activityIdGenerator;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IStringLocalizer S;

    public WorkflowApiService(
        IWorkflowTypeStore workflowTypeStore,
        IWorkflowStore workflowStore,
        IWorkflowManager workflowManager,
        IActivityLibrary activityLibrary,
        IWorkflowTypeIdGenerator workflowTypeIdGenerator,
        IActivityIdGenerator activityIdGenerator,
        IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions,
        IStringLocalizer<WorkflowApiService> stringLocalizer)
    {
        _workflowTypeStore = workflowTypeStore;
        _workflowStore = workflowStore;
        _workflowManager = workflowManager;
        _activityLibrary = activityLibrary;
        _workflowTypeIdGenerator = workflowTypeIdGenerator;
        _activityIdGenerator = activityIdGenerator;
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
        S = stringLocalizer;
    }

    public async Task<IReadOnlyList<WorkflowTypeDto>> ListWorkflowTypesAsync()
        => (await _workflowTypeStore.ListAsync())
            .Select(ToDto)
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public async Task<WorkflowTypeDto> GetWorkflowTypeAsync(string workflowTypeId)
    {
        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);
        return workflowType is null ? null : ToDto(workflowType);
    }

    public async Task<WorkflowTypeDto> CreateWorkflowTypeAsync(WorkflowTypeDto model, ModelStateDictionary modelState)
    {
        Normalize(model);
        AssignStableActivityIds(model);
        if (model.WorkflowTypeId is not null)
        {
            var existing = await _workflowTypeStore.GetAsync(model.WorkflowTypeId);
            if (existing is not null)
            {
                var existingDto = ToDto(existing);
                if (!AreEquivalent(model, existingDto))
                {
                    modelState.AddModelError(nameof(WorkflowTypeDto.WorkflowTypeId), S["A workflow type with the same identifier already exists with a different definition."]);
                }

                return existingDto;
            }
        }

        var workflowType = ToModel(model, new WorkflowType());
        await ValidateWorkflowTypeAsync(workflowType, modelState);

        if (!modelState.IsValid)
        {
            return null;
        }

        workflowType.WorkflowTypeId ??= _workflowTypeIdGenerator.GenerateUniqueId(workflowType);
        await _workflowTypeStore.SaveAsync(workflowType);
        return ToDto(workflowType);
    }

    public async Task<WorkflowTypeDto> UpdateWorkflowTypeAsync(string workflowTypeId, WorkflowTypeDto model, ModelStateDictionary modelState)
    {
        Normalize(model, workflowTypeId);
        var existing = await _workflowTypeStore.GetAsync(workflowTypeId);
        if (existing is null)
        {
            return null;
        }

        var workflowType = ToModel(model, existing);
        await ValidateWorkflowTypeAsync(workflowType, modelState, existingWorkflowTypeId: workflowTypeId);

        if (!modelState.IsValid)
        {
            return ToDto(existing);
        }

        await _workflowTypeStore.SaveAsync(workflowType);
        return ToDto(workflowType);
    }

    public async Task<bool> DeleteWorkflowTypeAsync(string workflowTypeId)
    {
        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);
        if (workflowType is null)
        {
            return false;
        }

        await _workflowTypeStore.DeleteAsync(workflowType);
        return true;
    }

    public async Task<WorkflowGraphValidationResponse> ValidateWorkflowTypeAsync(WorkflowTypeDto model)
    {
        Normalize(model);
        var modelState = new ModelStateDictionary();
        var workflowType = ToModel(model, new WorkflowType());
        await ValidateWorkflowTypeAsync(workflowType, modelState);

        return new WorkflowGraphValidationResponse
        {
            IsValid = modelState.IsValid,
            Errors = modelState.Values.SelectMany(x => x.Errors.Select(e => e.ErrorMessage)).ToArray(),
        };
    }

    public async Task<IReadOnlyList<WorkflowActivityTypeDescriptor>> ListActivityTypesAsync()
    {
        var activities = new List<WorkflowActivityTypeDescriptor>();

        foreach (var activity in _activityLibrary.ListActivities().OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
        {
            activities.Add(await DescribeActivityAsync(activity));
        }

        return activities;
    }

    public async Task<WorkflowActivityTypeDescriptor> GetActivityTypeAsync(string name)
    {
        var activity = _activityLibrary.ListActivities().FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        return activity is null ? null : await DescribeActivityAsync(activity);
    }

    public async Task<WorkflowTypeDto> SetEnabledAsync(string workflowTypeId, bool isEnabled)
    {
        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);
        if (workflowType is null)
        {
            return null;
        }

        workflowType.IsEnabled = isEnabled;
        await _workflowTypeStore.SaveAsync(workflowType);
        return ToDto(workflowType);
    }

    public async Task<WorkflowExecutionResponse> ExecuteAsync(string workflowTypeId, WorkflowExecutionRequest request, ModelStateDictionary modelState)
    {
        var workflowType = await _workflowTypeStore.GetAsync(workflowTypeId);
        if (workflowType is null)
        {
            return null;
        }

        if (!workflowType.IsEnabled)
        {
            modelState.AddModelError(nameof(WorkflowType.IsEnabled), S["The workflow type must be enabled before it can be executed."]);
            return new WorkflowExecutionResponse();
        }

        ActivityRecord startActivity = null;
        if (!string.IsNullOrWhiteSpace(request?.StartActivityId))
        {
            startActivity = workflowType.Activities.FirstOrDefault(x => string.Equals(x.ActivityId, request.StartActivityId, StringComparison.OrdinalIgnoreCase));
            if (startActivity is null)
            {
                modelState.AddModelError(nameof(WorkflowExecutionRequest.StartActivityId), S["The specified start activity does not exist."]);
                return new WorkflowExecutionResponse();
            }
        }

        var context = await _workflowManager.StartWorkflowAsync(workflowType, startActivity, ToDictionary(request?.Input), request?.CorrelationId);

        return new WorkflowExecutionResponse
        {
            Workflow = context.Workflow,
            Output = ToJsonObject(context.Output),
            ExecutedActivityIds = context.ExecutedActivities.Select(x => x.ActivityId).Reverse().ToArray(),
        };
    }

    public async Task<WorkflowInstancesResponse> ListInstancesAsync(string workflowTypeId, int skip, int take)
    {
        skip = Math.Max(skip, 0);
        take = Math.Clamp(take, 1, 200);

        return new WorkflowInstancesResponse
        {
            Skip = skip,
            Take = take,
            TotalCount = await _workflowStore.CountAsync(workflowTypeId),
            Items = (await _workflowStore.ListAsync(workflowTypeId, skip, take)).ToList(),
        };
    }

    public Task<Workflow> GetInstanceAsync(string workflowId)
        => _workflowStore.GetAsync(workflowId);

    public async Task<bool> CancelInstanceAsync(string workflowId)
    {
        var workflow = await _workflowStore.GetAsync(workflowId);
        if (workflow is null)
        {
            return false;
        }

        await _workflowStore.DeleteAsync(workflow);
        return true;
    }

    private async Task ValidateWorkflowTypeAsync(WorkflowType workflowType, ModelStateDictionary modelState, string existingWorkflowTypeId = null)
    {
        if (string.IsNullOrWhiteSpace(workflowType.Name))
        {
            modelState.AddModelError(nameof(WorkflowTypeDto.Name), S["The workflow type name is required."]);
        }

        var existingTypes = await _workflowTypeStore.ListAsync();
        if (existingTypes.Any(x => string.Equals(x.Name, workflowType.Name, StringComparison.OrdinalIgnoreCase) && !string.Equals(x.WorkflowTypeId, existingWorkflowTypeId, StringComparison.OrdinalIgnoreCase)))
        {
            modelState.AddModelError(nameof(WorkflowTypeDto.Name), S["A workflow type with the same name already exists."]);
        }

        if (!string.IsNullOrWhiteSpace(workflowType.WorkflowTypeId)
            && existingTypes.Any(x => string.Equals(x.WorkflowTypeId, workflowType.WorkflowTypeId, StringComparison.OrdinalIgnoreCase) && !string.Equals(x.WorkflowTypeId, existingWorkflowTypeId, StringComparison.OrdinalIgnoreCase)))
        {
            modelState.AddModelError(nameof(WorkflowTypeDto.WorkflowTypeId), S["A workflow type with the same identifier already exists."]);
        }

        if (workflowType.Activities.Count == 0)
        {
            modelState.AddModelError(nameof(WorkflowTypeDto.Activities), S["A workflow type must contain at least one activity."]);
            return;
        }

        if (!workflowType.Activities.Any(x => x.IsStart))
        {
            modelState.AddModelError(nameof(WorkflowTypeDto.Activities), S["A workflow type must contain at least one start activity."]);
        }

        var activityIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < workflowType.Activities.Count; i++)
        {
            var activity = workflowType.Activities[i];
            if (string.IsNullOrWhiteSpace(activity.Name))
            {
                modelState.AddModelError($"activities[{i}].name", S["The activity name is required."]);
                continue;
            }

            if (_activityLibrary.GetActivityByName(activity.Name) is null)
            {
                modelState.AddModelError($"activities[{i}].name", S["The specified activity type does not exist."]);
            }

            if (string.IsNullOrWhiteSpace(activity.ActivityId))
            {
                activity.ActivityId = _activityIdGenerator.GenerateUniqueId(activity);
            }

            if (!activityIds.Add(activity.ActivityId))
            {
                modelState.AddModelError($"activities[{i}].activityId", S["Activity identifiers must be unique."]);
            }
        }

        for (var i = 0; i < workflowType.Transitions.Count; i++)
        {
            var transition = workflowType.Transitions[i];

            if (string.IsNullOrWhiteSpace(transition.SourceActivityId) || !activityIds.Contains(transition.SourceActivityId))
            {
                modelState.AddModelError($"transitions[{i}].sourceActivityId", S["The source activity must exist."]);
            }

            if (string.IsNullOrWhiteSpace(transition.DestinationActivityId) || !activityIds.Contains(transition.DestinationActivityId))
            {
                modelState.AddModelError($"transitions[{i}].destinationActivityId", S["The destination activity must exist."]);
            }

            if (string.IsNullOrWhiteSpace(transition.SourceOutcomeName))
            {
                modelState.AddModelError($"transitions[{i}].sourceOutcomeName", S["The source outcome name is required."]);
            }
        }
    }

    private async Task<WorkflowActivityTypeDescriptor> DescribeActivityAsync(IActivity activity)
    {
        var defaultProperties = WorkflowSchemaBuilder.MaterializePropertyValues(activity, _jsonSerializerOptions);
        var propertiesSchema = WorkflowSchemaBuilder.BuildPropertiesSchema(activity, defaultProperties, _jsonSerializerOptions);
        var activityRecord = new ActivityRecord
        {
            ActivityId = _activityIdGenerator.GenerateUniqueId(new ActivityRecord()),
            Name = activity.Name,
            IsStart = activity.IsEvent(),
            Properties = activity.Properties,
        };
        var workflowType = new WorkflowType
        {
            Name = "Schema",
            WorkflowTypeId = "schema",
            Activities = [activityRecord],
            Transitions = [],
        };
        var workflow = _workflowManager.NewWorkflow(workflowType);
        var workflowContext = await _workflowManager.CreateWorkflowExecutionContextAsync(workflowType, workflow);
        var activityContext = await _workflowManager.CreateActivityExecutionContextAsync(activityRecord, activity.Properties);

        return new WorkflowActivityTypeDescriptor
        {
            Name = activity.Name,
            DisplayText = activity.DisplayText.Value,
            Category = activity.Category.Value,
            HasEditor = activity.HasEditor,
            Outcomes = (await activity.GetPossibleOutcomesAsync(workflowContext, activityContext)).Select(x => x.Name).ToArray(),
            Schema = propertiesSchema.DeepClone().AsObject(),
            PropertiesSchema = propertiesSchema,
            ActivityRecordSchema = WorkflowSchemaBuilder.BuildActivityRecordSchema(activity, propertiesSchema, _jsonSerializerOptions),
            DefaultProperties = defaultProperties,
        };
    }

    private static WorkflowType ToModel(WorkflowTypeDto model, WorkflowType workflowType)
    {
        workflowType.WorkflowTypeId = model.WorkflowTypeId;
        workflowType.Name = model.Name?.Trim();
        workflowType.IsEnabled = model.IsEnabled;
        workflowType.IsSingleton = model.IsSingleton;
        workflowType.LockTimeout = model.LockTimeout;
        workflowType.LockExpiration = model.LockExpiration;
        workflowType.DeleteFinishedWorkflows = model.DeleteFinishedWorkflows;
        workflowType.Activities = model.Activities.Select(activity => new ActivityRecord
        {
            ActivityId = activity.ActivityId,
            Name = activity.Name?.Trim(),
            X = activity.X,
            Y = activity.Y,
            IsStart = activity.IsStart,
            Properties = Clone(activity.Properties),
        }).ToList();
        workflowType.Transitions = model.Transitions.Select(transition => new Transition
        {
            SourceActivityId = transition.SourceActivityId?.Trim(),
            SourceOutcomeName = transition.SourceOutcomeName?.Trim(),
            DestinationActivityId = transition.DestinationActivityId?.Trim(),
        }).ToList();

        return workflowType;
    }

    private static WorkflowTypeDto ToDto(WorkflowType workflowType)
        => new()
        {
            WorkflowTypeId = workflowType.WorkflowTypeId,
            Name = workflowType.Name,
            IsEnabled = workflowType.IsEnabled,
            IsSingleton = workflowType.IsSingleton,
            LockTimeout = workflowType.LockTimeout,
            LockExpiration = workflowType.LockExpiration,
            DeleteFinishedWorkflows = workflowType.DeleteFinishedWorkflows,
            Activities = workflowType.Activities.Select(activity => new ActivityRecordDto
            {
                ActivityId = activity.ActivityId,
                Name = activity.Name,
                X = activity.X,
                Y = activity.Y,
                IsStart = activity.IsStart,
                Properties = Clone(activity.Properties),
            }).ToList(),
            Transitions = workflowType.Transitions.Select(transition => new TransitionDto
            {
                SourceActivityId = transition.SourceActivityId,
                SourceOutcomeName = transition.SourceOutcomeName,
                DestinationActivityId = transition.DestinationActivityId,
            }).ToList(),
        };

    private static void Normalize(WorkflowTypeDto model, string workflowTypeId = null)
    {
        model.WorkflowTypeId = string.IsNullOrWhiteSpace(model.WorkflowTypeId) ? workflowTypeId : model.WorkflowTypeId.Trim();
        model.Name = model.Name?.Trim();
        model.Activities ??= [];
        model.Transitions ??= [];

        foreach (var activity in model.Activities)
        {
            activity.ActivityId = activity.ActivityId?.Trim();
            activity.Name = activity.Name?.Trim();
            activity.Properties ??= [];
        }

        foreach (var transition in model.Transitions)
        {
            transition.SourceActivityId = transition.SourceActivityId?.Trim();
            transition.SourceOutcomeName = transition.SourceOutcomeName?.Trim();
            transition.DestinationActivityId = transition.DestinationActivityId?.Trim();
        }
    }

    private static void AssignStableActivityIds(WorkflowTypeDto model)
    {
        if (string.IsNullOrEmpty(model.WorkflowTypeId))
        {
            return;
        }

        for (var index = 0; index < model.Activities.Count; index++)
        {
            model.Activities[index].ActivityId ??= $"{model.WorkflowTypeId}-activity-{index + 1}";
        }
    }

    private bool AreEquivalent(WorkflowTypeDto left, WorkflowTypeDto right)
        => JsonNode.DeepEquals(
            JsonSerializer.SerializeToNode(left, _jsonSerializerOptions),
            JsonSerializer.SerializeToNode(right, _jsonSerializerOptions));

    private static JsonObject Clone(JsonObject value)
        => value?.DeepClone() as JsonObject ?? [];

    private static Dictionary<string, object> ToDictionary(JsonObject input)
        => input is null
            ? new Dictionary<string, object>()
            : JsonSerializer.Deserialize<Dictionary<string, object>>(input.ToJsonString()) ?? new Dictionary<string, object>();

    private static JsonObject ToJsonObject(IDictionary<string, object> output)
        => output is null
            ? []
            : JsonSerializer.SerializeToNode(output) as JsonObject ?? [];
}

internal sealed class WorkflowRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
            [
                new RemoteManagementCapability
                {
                    Id = "workflows",
                    Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
                    DisplayName = "Workflows",
                },
            ]);
}

internal static class WorkflowSchemaBuilder
{
    public static JsonObject BuildPropertiesSchema(
        IActivity activity,
        JsonObject defaults,
        JsonSerializerOptions jsonSerializerOptions)
    {
        var properties = new JsonObject();

        foreach (var property in GetPersistedProperties(activity))
        {
            var propertySchema = GetJsonSchema(property.PropertyType, jsonSerializerOptions);

            if (property.GetCustomAttribute<DescriptionAttribute>() is { } description)
            {
                propertySchema["description"] = description.Description;
            }

            if (defaults.TryGetPropertyValue(property.Name, out var defaultValue))
            {
                propertySchema["default"] = defaultValue?.DeepClone();
            }

            RebaseLocalReferences(
                propertySchema,
                $"/properties/{EscapeJsonPointerSegment(property.Name)}");
            properties[property.Name] = propertySchema;
        }

        return new()
        {
            ["type"] = "object",
            ["properties"] = properties,
        };
    }

    public static JsonObject BuildActivityRecordSchema(
        IActivity activity,
        JsonObject propertiesSchema,
        JsonSerializerOptions jsonSerializerOptions)
    {
        var schema = GetJsonSchema(typeof(ActivityRecord), jsonSerializerOptions);
        var properties = schema["properties"] as JsonObject;

        if (properties is null)
        {
            properties = [];
            schema["properties"] = properties;
        }

        var nameProperty = GetJsonPropertyName(jsonSerializerOptions, typeof(ActivityRecord), nameof(ActivityRecord.Name));
        var activityIdProperty = GetJsonPropertyName(jsonSerializerOptions, typeof(ActivityRecord), nameof(ActivityRecord.ActivityId));
        var isStartProperty = GetJsonPropertyName(jsonSerializerOptions, typeof(ActivityRecord), nameof(ActivityRecord.IsStart));
        var xProperty = GetJsonPropertyName(jsonSerializerOptions, typeof(ActivityRecord), nameof(ActivityRecord.X));
        var yProperty = GetJsonPropertyName(jsonSerializerOptions, typeof(ActivityRecord), nameof(ActivityRecord.Y));
        var propertiesProperty = GetJsonPropertyName(jsonSerializerOptions, typeof(ActivityRecord), nameof(ActivityRecord.Properties));

        SetSchemaKeyword(properties, nameProperty, "const", JsonValue.Create(activity.Name));
        SetSchemaKeyword(properties, isStartProperty, "default", JsonValue.Create(activity is IEvent));
        SetSchemaKeyword(properties, xProperty, "default", JsonValue.Create(0));
        SetSchemaKeyword(properties, yProperty, "default", JsonValue.Create(0));

        var nestedPropertiesSchema = propertiesSchema.DeepClone();
        RebaseLocalReferences(
            nestedPropertiesSchema,
            $"/properties/{EscapeJsonPointerSegment(propertiesProperty)}");
        properties[propertiesProperty] = nestedPropertiesSchema;
        schema["required"] = new JsonArray(nameProperty, activityIdProperty);

        return schema;
    }

    public static JsonObject MaterializePropertyValues(IActivity activity, JsonSerializerOptions jsonSerializerOptions)
    {
        var result = new JsonObject();

        foreach (var property in GetPersistedProperties(activity))
        {
            try
            {
                var value = property.GetValue(activity);
                result[property.Name] = value is null
                    ? null
                    : JsonSerializer.SerializeToNode(value, property.PropertyType, jsonSerializerOptions);
            }
            catch (Exception exception) when (exception is
                TargetInvocationException or
                JsonException or
                NotSupportedException)
            {
                throw new InvalidOperationException(
                    $"The default value for activity property '{activity.Name}.{property.Name}' could not be read.",
                    exception);
            }
        }

        return result;
    }

    private static JsonObject GetJsonSchema(Type type, JsonSerializerOptions jsonSerializerOptions)
    {
        var schema = jsonSerializerOptions.GetJsonSchemaAsNode(
            type,
            new JsonSchemaExporterOptions
            {
                TransformSchemaNode = static (context, node) =>
                {
                    if (context.PropertyInfo?.AttributeProvider is MemberInfo member &&
                        member.GetCustomAttribute<DescriptionAttribute>() is { } description)
                    {
                        var transformed = node as JsonObject ?? [];
                        transformed["description"] = description.Description;

                        return transformed;
                    }

                    return node;
                },
            });

        return schema as JsonObject ?? [];
    }

    private static void RebaseLocalReferences(JsonNode node, string prefix)
    {
        if (node is JsonObject jsonObject)
        {
            if (jsonObject["$ref"]?.GetValue<string>() is { } reference &&
                reference.StartsWith('#'))
            {
                jsonObject["$ref"] = $"#{prefix}{reference[1..]}";
            }

            foreach (var property in jsonObject)
            {
                if (property.Value is not null)
                {
                    RebaseLocalReferences(property.Value, prefix);
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item is not null)
                {
                    RebaseLocalReferences(item, prefix);
                }
            }
        }
    }

    private static string EscapeJsonPointerSegment(string value) =>
        value.Replace("~", "~0", StringComparison.Ordinal)
            .Replace("/", "~1", StringComparison.Ordinal);

    private static string GetJsonPropertyName(
        JsonSerializerOptions jsonSerializerOptions,
        Type declaringType,
        string propertyName)
    {
        var property = jsonSerializerOptions
            .GetTypeInfo(declaringType)
            .Properties
            .FirstOrDefault(property =>
                property.AttributeProvider is PropertyInfo propertyInfo &&
                propertyInfo.Name == propertyName);

        return property?.Name ?? propertyName;
    }

    private static void SetSchemaKeyword(
        JsonObject properties,
        string propertyName,
        string keyword,
        JsonNode value)
    {
        var schema = properties[propertyName] as JsonObject ?? [];
        schema[keyword] = value;
        properties[propertyName] = schema;
    }

    private static PropertyInfo[] GetPersistedProperties(IActivity activity)
        => activity.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.CanRead &&
                property.SetMethod?.IsPublic == true &&
                property.GetIndexParameters().Length == 0 &&
                property.Name != nameof(IActivity.Properties))
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();
}
