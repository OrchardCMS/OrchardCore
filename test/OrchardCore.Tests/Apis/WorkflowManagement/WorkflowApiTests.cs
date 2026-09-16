using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Json;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Tests.Apis.WorkflowManagement;

public class WorkflowApiTests
{
    [Fact]
    public async Task WorkflowService_CreatesExecutesListsAndCancelsInstance()
    {
        using var context = new SiteContext();

        await context.InitializeAsync();
        await EnableFeaturesAsync(context, "OrchardCore.Workflows");
        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService(GetServiceType("OrchardCore.Workflows.Services.WorkflowApiService", "OrchardCore.Workflows"));

            var workflowType = await InvokeAsync<WorkflowTypeDto>(service, "CreateWorkflowTypeAsync", new WorkflowTypeDto
            {
                Name = "Remote Workflow",
                IsEnabled = true,
                Activities =
                [
                    new ActivityRecordDto
                    {
                        Name = "SetOutputTask",
                        IsStart = true,
                        Properties = new JsonObject
                        {
                            ["OutputName"] = "result",
                            ["Syntax"] = "JavaScript",
                            ["Value"] = new JsonObject
                            {
                                ["Expression"] = "42",
                            },
                        },
                    },
                ],
            }, new ModelStateDictionary());

            var validation = await InvokeAsync<WorkflowGraphValidationResponse>(service, "ValidateWorkflowTypeAsync", new WorkflowTypeDto
            {
                Name = "Invalid Workflow",
                Activities =
                [
                    new ActivityRecordDto
                    {
                        ActivityId = "set-output",
                        Name = "SetOutputTask",
                        IsStart = true,
                    },
                ],
                Transitions =
                [
                    new TransitionDto
                    {
                        SourceActivityId = "set-output",
                        SourceOutcomeName = "Done",
                        DestinationActivityId = "missing",
                    },
                ],
            });

            var activityType = await InvokeAsync<WorkflowActivityTypeDescriptor>(service, "GetActivityTypeAsync", "SetOutputTask");
            var activityTypes = await InvokeAsync<IReadOnlyList<WorkflowActivityTypeDescriptor>>(service, "ListActivityTypesAsync");
            var execution = await InvokeAsync<WorkflowExecutionResponse>(service, "ExecuteAsync", workflowType.WorkflowTypeId, new WorkflowExecutionRequest(), new ModelStateDictionary());
            var instances = await InvokeAsync<WorkflowInstancesResponse>(service, "ListInstancesAsync", workflowType.WorkflowTypeId, 0, 10);

            Assert.NotNull(workflowType);
            Assert.False(validation.IsValid);
            Assert.Contains(validation.Errors, error => error.Contains("destination activity", StringComparison.OrdinalIgnoreCase));
            Assert.Contains("Done", activityType.Outcomes);
            Assert.Contains(activityTypes, descriptor => descriptor.Name == nameof(SetOutputTask));
            Assert.All(activityTypes, descriptor => Assert.Equal("object", descriptor.PropertiesSchema["type"]?.GetValue<string>()));
            Assert.True(activityType.Schema.ContainsKey("properties"));
            Assert.Equal(activityType.Schema.ToJsonString(), activityType.PropertiesSchema.ToJsonString());
            Assert.Equal("object", activityType.PropertiesSchema["type"]?.GetValue<string>());
            Assert.True(activityType.DefaultProperties.ContainsKey(nameof(SetOutputTask.Syntax)));
            Assert.Equal(
                nameof(SetOutputTask),
                activityType.ActivityRecordSchema["properties"]?[nameof(ActivityRecord.Name)]?["const"]?.GetValue<string>());
            Assert.False(
                activityType.ActivityRecordSchema["properties"]?[nameof(ActivityRecord.IsStart)]?["default"]?.GetValue<bool>());
            Assert.Equal(
                [nameof(ActivityRecord.Name), nameof(ActivityRecord.ActivityId)],
                activityType.ActivityRecordSchema["required"]?.AsArray().Select(node => node?.GetValue<string>()));
            Assert.Equal("42", execution.Output["result"]?.ToString());
            Assert.Equal(1, instances.TotalCount);

            var workflowId = Assert.Single(instances.Items).WorkflowId;
            Assert.False(string.IsNullOrWhiteSpace(workflowId));
        });
    }

    [Fact]
    public async Task WorkflowService_ExplicitStableIdCreateIsRetrySafe()
    {
        using var context = new SiteContext();

        await context.InitializeAsync();
        await EnableFeaturesAsync(context, "OrchardCore.Workflows");
        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);

        var definition = new WorkflowTypeDto
        {
            WorkflowTypeId = "retry-safe-workflow",
            Name = "Retry Safe Workflow",
            IsEnabled = true,
            Activities =
            [
                new ActivityRecordDto
                {
                    Name = "SetOutputTask",
                    IsStart = true,
                    Properties = new JsonObject
                    {
                        ["OutputName"] = "result",
                        ["Syntax"] = "JavaScript",
                        ["Value"] = new JsonObject
                        {
                            ["Expression"] = "42",
                        },
                    },
                },
            ],
        };

        WorkflowTypeDto first = null;
        await context.UsingTenantScopeAsync(async scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService(GetServiceType("OrchardCore.Workflows.Services.WorkflowApiService", "OrchardCore.Workflows"));
            var firstState = new ModelStateDictionary();
            first = await InvokeAsync<WorkflowTypeDto>(service, "CreateWorkflowTypeAsync", definition, firstState);
            Assert.True(firstState.IsValid);
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService(GetServiceType("OrchardCore.Workflows.Services.WorkflowApiService", "OrchardCore.Workflows"));
            var retryState = new ModelStateDictionary();
            definition.Activities[0].ActivityId = null;
            var retry = await InvokeAsync<WorkflowTypeDto>(service, "CreateWorkflowTypeAsync", definition, retryState);
            Assert.True(retryState.IsValid);
            Assert.Equal(first.WorkflowTypeId, retry.WorkflowTypeId);
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService(GetServiceType("OrchardCore.Workflows.Services.WorkflowApiService", "OrchardCore.Workflows"));
            var conflictState = new ModelStateDictionary();
            var conflict = await InvokeAsync<WorkflowTypeDto>(service, "CreateWorkflowTypeAsync", new WorkflowTypeDto
            {
                WorkflowTypeId = definition.WorkflowTypeId,
                Name = definition.Name,
                IsEnabled = false,
                Activities = definition.Activities,
            }, conflictState);

            Assert.False(conflictState.IsValid);
            Assert.True(conflict.IsEnabled);
            Assert.Single(await InvokeAsync<IReadOnlyList<WorkflowTypeDto>>(service, "ListWorkflowTypesAsync"), x => x.WorkflowTypeId == definition.WorkflowTypeId);
            Assert.True(await InvokeAsync<bool>(service, "DeleteWorkflowTypeAsync", definition.WorkflowTypeId));
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService(GetServiceType("OrchardCore.Workflows.Services.WorkflowApiService", "OrchardCore.Workflows"));
            Assert.False(await InvokeAsync<bool>(service, "DeleteWorkflowTypeAsync", definition.WorkflowTypeId));
        });
    }

    [Fact]
    public void WorkflowSchemaBuilder_RecursivePersistedProperties_ExportsDescriptionsDefaultsAndRebasedReferences()
    {
        var schemaBuilderType = GetServiceType("OrchardCore.Workflows.Services.WorkflowSchemaBuilder", "OrchardCore.Workflows");
        var serializerOptions = new DocumentJsonSerializerOptions().SerializerOptions;
        var activity = new SchemaTestTask();
        var defaults = InvokeStatic<JsonObject>(
            schemaBuilderType,
            "MaterializePropertyValues",
            activity,
            serializerOptions);
        var propertiesSchema = InvokeStatic<JsonObject>(
            schemaBuilderType,
            "BuildPropertiesSchema",
            activity,
            defaults,
            serializerOptions);
        var activityRecordSchema = InvokeStatic<JsonObject>(
            schemaBuilderType,
            "BuildActivityRecordSchema",
            activity,
            propertiesSchema,
            serializerOptions);
        var retryCountSchema = propertiesSchema["properties"]?[nameof(SchemaTestTask.RetryCount)];

        Assert.Equal("integer", retryCountSchema?["type"]?.GetValue<string>());
        Assert.Equal("The number of retry attempts.", retryCountSchema?["description"]?.GetValue<string>());
        Assert.Equal(3, retryCountSchema?["default"]?.GetValue<int>());
        Assert.Equal(3, defaults[nameof(SchemaTestTask.RetryCount)]?.GetValue<int>());
        Assert.Contains("The next recursive node.", propertiesSchema.ToJsonString());

        var propertyReferences = EnumerateReferences(propertiesSchema).ToArray();
        var activityRecordReferences = EnumerateReferences(activityRecordSchema).ToArray();

        Assert.NotEmpty(propertyReferences);
        Assert.NotEmpty(activityRecordReferences);
        Assert.All(
            propertyReferences,
            reference => Assert.StartsWith($"#/properties/{nameof(SchemaTestTask.Recursive)}", reference));
        Assert.All(
            activityRecordReferences,
            reference => Assert.StartsWith(
                $"#/properties/{nameof(ActivityRecord.Properties)}/properties/{nameof(SchemaTestTask.Recursive)}",
                reference));
    }

    private static async Task EnableFeaturesAsync(SiteContext context, params string[] featureIds)
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var features = (await shellFeaturesManager.GetAvailableFeaturesAsync())
                .Where(feature => featureIds.Contains(feature.Id, StringComparer.Ordinal))
                .ToArray();

            await shellFeaturesManager.EnableFeaturesAsync(features, force: true);
        });
    }

    private static Type GetServiceType(string fullName, string assemblyName)
        => Type.GetType($"{fullName}, {assemblyName}", throwOnError: true)!;

    private static async Task<T> InvokeAsync<T>(object target, string methodName, params object[] arguments)
    {
        var method = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length);
        var task = (Task)method.Invoke(target, arguments)!;
        await task;

        return (T)task.GetType().GetProperty("Result")!.GetValue(task)!;
    }

    private static T InvokeStatic<T>(Type targetType, string methodName, params object[] arguments)
    {
        var method = targetType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length);

        return (T)method.Invoke(null, arguments)!;
    }

    private static IEnumerable<string> EnumerateReferences(JsonNode node)
    {
        if (node is JsonObject jsonObject)
        {
            foreach (var property in jsonObject)
            {
                if (property.Key == "$ref")
                {
                    yield return property.Value!.GetValue<string>();
                }
                else if (property.Value is not null)
                {
                    foreach (var reference in EnumerateReferences(property.Value))
                    {
                        yield return reference;
                    }
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item is not null)
                {
                    foreach (var reference in EnumerateReferences(item))
                    {
                        yield return reference;
                    }
                }
            }
        }
    }

    private sealed class SchemaTestTask : TaskActivity<SchemaTestTask>
    {
        public override LocalizedString DisplayText => new(nameof(SchemaTestTask), "Schema Test Task");

        public override LocalizedString Category => new("Tests", "Tests");

        [Description("The number of retry attempts.")]
        public int RetryCount
        {
            get => GetProperty(() => 3);
            set => SetProperty(value);
        }

        public RecursiveNode Recursive
        {
            get => GetProperty<RecursiveNode>();
            set => SetProperty(value);
        }
    }

    private sealed class RecursiveNode
    {
        [Description("The next recursive node.")]
        public RecursiveNode Child { get; set; }
    }
}
