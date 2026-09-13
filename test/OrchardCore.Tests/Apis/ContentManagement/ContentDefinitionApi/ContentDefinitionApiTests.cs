using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Settings;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentTypes.Models;
using OrchardCore.Environment.Shell;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.ContentManagement.ContentDefinitionApi;

public class ContentDefinitionApiTests
{
    [Fact]
    public void ContentDefinitionSchemaBuilder_DescribedUnconstrainedProperty_PreservesBooleanSchema()
    {
        var builderType = GetServiceType("OrchardCore.ContentTypes.Services.ContentDefinitionSchemaBuilder", "OrchardCore.ContentTypes");
        var method = builderType.GetMethod("BuildSchema", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
        var schema = (JsonObject)method.Invoke(null, [typeof(DescribedSchema), JsonSerializerOptions.Web])!;
        var value = schema["properties"]!["value"]!;
        Assert.Equal("An arbitrary JSON value.", value["description"]!.GetValue<string>());
        Assert.True(value["allOf"]![0]!.GetValue<bool>());
    }

    private sealed class DescribedSchema
    {
        [System.ComponentModel.Description("An arbitrary JSON value.")]
        public object Value { get; set; }
    }

    [Fact]
    public void ContentPartDefinitionSchema_DescribesKnownSettings()
    {
        var schema = JsonSerializerOptions.Web.GetJsonSchemaAsNode(typeof(ContentPartDefinitionDto));

        var contentPartSettings = schema["properties"]!["settings"]!["properties"]![nameof(ContentPartSettings)]!;
        Assert.Equal("boolean", contentPartSettings["properties"]!["attachable"]!["type"]!.GetValue<string>());
        Assert.Equal("boolean", contentPartSettings["properties"]!["reusable"]!["type"]!.GetValue<string>());
    }

    [Fact]
    public void ContentDefinitionSchemaBuilder_RepresentsDictionariesAsObjects()
    {
        var builderType = GetServiceType("OrchardCore.ContentTypes.Services.ContentDefinitionSchemaBuilder", "OrchardCore.ContentTypes");
        var method = builderType.GetMethod("BuildSchema", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
        var schema = (JsonObject)method.Invoke(null, [typeof(DictionarySchema), JsonSerializerOptions.Web])!;

        var values = schema["properties"]!["values"]!;
        Assert.Contains("object", values["type"]!.ToJsonString(), StringComparison.Ordinal);
        Assert.NotNull(values["additionalProperties"]);
    }

    [Fact]
    public async Task ContentDefinitionService_CreatesAndReadsDefinitions()
    {
        using var context = new SiteContext();

        await context.InitializeAsync();
        await EnableFeaturesAsync(context, "OrchardCore.ContentTypes", "OrchardCore.ContentFields");
        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService(GetServiceType("OrchardCore.ContentTypes.Services.ContentDefinitionApiService", "OrchardCore.ContentTypes"));
            var fieldTypes = Invoke<IReadOnlyList<ContentDefinitionFieldTypeDescriptor>>(service, "ListFieldTypes");
            var fieldTypeName = Assert.Single(fieldTypes, x => x.Name == "TextField").Name;

            var part = await InvokeAsync<ContentPartDefinitionDto>(service, "CreatePartAsync", new ContentPartDefinitionDto
            {
                Name = "RemoteTextPart",
                Settings = new ContentPartDefinitionSettingsDto
                {
                    ContentPartSettings = new ContentPartSettings
                    {
                        Attachable = true,
                        Reusable = true,
                    },
                },
            }, new ModelStateDictionary());
            var repeatedPartState = new ModelStateDictionary();
            var repeatedPart = await InvokeAsync<ContentPartDefinitionDto>(service, "CreatePartAsync", part, repeatedPartState);

            var field = await InvokeAsync<ContentPartFieldDefinitionDto>(service, "CreateFieldAsync", "RemoteTextPart", new ContentPartFieldDefinitionDto
            {
                Name = "RemoteText",
                FieldName = fieldTypeName,
            }, new ModelStateDictionary());
            var repeatedFieldState = new ModelStateDictionary();
            var repeatedField = await InvokeAsync<ContentPartFieldDefinitionDto>(service, "CreateFieldAsync", "RemoteTextPart", field, repeatedFieldState);

            var type = await InvokeAsync<ContentTypeDefinitionDto>(service, "CreateTypeAsync", new ContentTypeDefinitionDto
            {
                Name = "RemoteApiType",
                DisplayName = "Remote Api Type",
                Parts =
                [
                    new ContentTypePartDefinitionDto
                    {
                        Name = "RemoteTextPart",
                        PartName = "RemoteTextPart",
                    },
                ],
            }, new ModelStateDictionary());
            var repeatedTypeState = new ModelStateDictionary();
            var repeatedType = await InvokeAsync<ContentTypeDefinitionDto>(service, "CreateTypeAsync", type, repeatedTypeState);

            Assert.NotNull(part);
            Assert.NotNull(field);
            Assert.NotNull(type);
            Assert.True(repeatedPartState.IsValid);
            Assert.True(repeatedFieldState.IsValid);
            Assert.True(repeatedTypeState.IsValid);
            Assert.Equal(part.Name, repeatedPart.Name);
            Assert.Equal(field.Name, repeatedField.Name);
            Assert.Equal(type.Name, repeatedType.Name);

            part = await InvokeAsync<ContentPartDefinitionDto>(service, "GetPartAsync", "RemoteTextPart");
            type = await InvokeAsync<ContentTypeDefinitionDto>(service, "GetTypeAsync", "RemoteApiType");

            Assert.True(part.Settings.ContentPartSettings.Attachable);
            Assert.True(part.Settings.ContentPartSettings.Reusable);
            Assert.Contains(part.Fields, createdField => createdField.Name == "RemoteText" && createdField.FieldName == fieldTypeName);
            Assert.Contains(type.Parts, createdPart => createdPart.Name == "RemoteTextPart" && createdPart.PartName == "RemoteTextPart");
            Assert.Single(await InvokeAsync<IReadOnlyList<ContentPartDefinitionDto>>(service, "ListPartsAsync"), x => x.Name == "RemoteTextPart");
            Assert.Single(await InvokeAsync<IReadOnlyList<ContentTypeDefinitionDto>>(service, "ListTypesAsync"), x => x.Name == "RemoteApiType");
            Assert.Single(await InvokeAsync<IReadOnlyList<ContentPartFieldDefinitionDto>>(service, "ListFieldsAsync", "RemoteTextPart"));

            var typeConflictState = new ModelStateDictionary();
            type.DisplayName = "Different Display Name";
            await InvokeAsync<ContentTypeDefinitionDto>(service, "CreateTypeAsync", type, typeConflictState);

            var partConflictState = new ModelStateDictionary();
            part.Settings.ContentPartSettings.Reusable = false;
            await InvokeAsync<ContentPartDefinitionDto>(service, "CreatePartAsync", part, partConflictState);

            var fieldConflictState = new ModelStateDictionary();
            field.FieldName = "NumericField";
            await InvokeAsync<ContentPartFieldDefinitionDto>(service, "CreateFieldAsync", "RemoteTextPart", field, fieldConflictState);

            Assert.False(typeConflictState.IsValid);
            Assert.False(partConflictState.IsValid);
            Assert.False(fieldConflictState.IsValid);

            var typeMismatchState = new ModelStateDictionary();
            type.Name = "RenamedRemoteApiType";
            await InvokeAsync<ContentTypeDefinitionDto>(service, "UpdateTypeAsync", "RemoteApiType", type, typeMismatchState);

            var partMismatchState = new ModelStateDictionary();
            part.Name = "RenamedRemoteTextPart";
            await InvokeAsync<ContentPartDefinitionDto>(service, "UpdatePartAsync", "RemoteTextPart", part, partMismatchState);

            var fieldMismatchState = new ModelStateDictionary();
            field.Name = "RenamedRemoteText";
            await InvokeAsync<ContentPartFieldDefinitionDto>(service, "UpdateFieldAsync", "RemoteTextPart", "RemoteText", field, fieldMismatchState);

            Assert.False(typeMismatchState.IsValid);
            Assert.False(partMismatchState.IsValid);
            Assert.False(fieldMismatchState.IsValid);
            Assert.NotNull(await InvokeAsync<ContentTypeDefinitionDto>(service, "GetTypeAsync", "RemoteApiType"));
            Assert.Null(await InvokeAsync<ContentTypeDefinitionDto>(service, "GetTypeAsync", "RenamedRemoteApiType"));
            Assert.NotNull(await InvokeAsync<ContentPartDefinitionDto>(service, "GetPartAsync", "RemoteTextPart"));
            Assert.Null(await InvokeAsync<ContentPartDefinitionDto>(service, "GetPartAsync", "RenamedRemoteTextPart"));
            Assert.NotNull(await InvokeAsync<ContentPartFieldDefinitionDto>(service, "GetFieldAsync", "RemoteTextPart", "RemoteText"));
            Assert.Null(await InvokeAsync<ContentPartFieldDefinitionDto>(service, "GetFieldAsync", "RemoteTextPart", "RenamedRemoteText"));
            Assert.True(await InvokeAsync<bool?>(service, "DeleteFieldAsync", "RemoteTextPart", "RemoteText"));
            Assert.False(await InvokeAsync<bool?>(service, "DeleteFieldAsync", "RemoteTextPart", "RemoteText"));
            Assert.True(await InvokeAsync<bool>(service, "DeleteTypeAsync", "RemoteApiType"));
            Assert.False(await InvokeAsync<bool>(service, "DeleteTypeAsync", "RemoteApiType"));
            Assert.True(await InvokeAsync<bool>(service, "DeletePartAsync", "RemoteTextPart"));
            Assert.False(await InvokeAsync<bool>(service, "DeletePartAsync", "RemoteTextPart"));
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContentPickerSettings_AreDiscoverableAndControlMultipleItemValidation(bool multiple)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await EnableFeaturesAsync(context, "OrchardCore.ContentTypes", "OrchardCore.ContentFields");
        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService(GetServiceType("OrchardCore.ContentTypes.Services.ContentDefinitionApiService", "OrchardCore.ContentTypes"));
            var schemas = Invoke<IReadOnlyList<ContentDefinitionSettingsSchemaDto>>(service, "ListSettingsSchemas");
            var picker = Assert.Single(schemas, schema => schema.Name == "ContentPickerFieldSettings");
            Assert.Equal("ContentPartField", picker.Scope);
            Assert.Equal("ContentPickerField", picker.AppliesTo);
            var discovered = Invoke<ContentDefinitionSettingsSchemaDto>(service, "GetSettingsSchema", picker.Name);
            Assert.True(JsonNode.DeepEquals(picker.Schema, discovered.Schema));
            var properties = discovered.Schema["properties"]!;
            Assert.Equal("boolean", properties["Multiple"]!["type"]!.GetValue<string>());
            Assert.Contains("Defaults to false", properties["Multiple"]!["description"]!.GetValue<string>());
            Assert.NotNull(properties["DisplayedContentTypes"]);
            Assert.NotNull(properties["Required"]);
            Assert.NotNull(properties["Hint"]);

            var state = new ModelStateDictionary();
            await InvokeAsync<ContentPartDefinitionDto>(service, "CreatePartAsync", new ContentPartDefinitionDto { Name = "Team" }, state);
            var field = JsonSerializer.Deserialize<ContentPartFieldDefinitionDto>("""
                {
                  "name": "Roster",
                  "fieldName": "ContentPickerField",
                  "settings": {
                    "ContentPickerFieldSettings": {
                      "DisplayedContentTypes": ["Person"]
                    }
                  }
                }
                """, JsonSerializerOptions.Web)!;
            // Leaving Multiple absent must retain the single-item default.
            if (multiple)
            {
                field.Settings.AdditionalSettings["ContentPickerFieldSettings"] = JsonSerializer.SerializeToElement(new
                {
                    Multiple = true,
                    DisplayedContentTypes = new[] { "Person" },
                });
            }

            await InvokeAsync<ContentPartFieldDefinitionDto>(service, "CreateFieldAsync", "Team", field, state);
            await InvokeAsync<ContentTypeDefinitionDto>(service, "CreateTypeAsync", new ContentTypeDefinitionDto
            {
                Name = "Team",
                DisplayName = "Team",
                Parts = [new ContentTypePartDefinitionDto { Name = "Team", PartName = "Team" }],
            }, state);
            Assert.True(state.IsValid);

            var stored = await InvokeAsync<ContentPartFieldDefinitionDto>(service, "GetFieldAsync", "Team", "Roster");
            var settings = stored.Settings.AdditionalSettings["ContentPickerFieldSettings"];
            Assert.Equal("Person", settings.GetProperty("DisplayedContentTypes")[0].GetString());
            if (multiple)
            {
                Assert.True(settings.GetProperty("Multiple").GetBoolean());
            }

            var manager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            var item = JsonSerializer.Deserialize<ContentItem>("""
                {
                  "ContentType": "Team",
                  "Team": {
                    "Roster": { "ContentItemIds": ["person-1", "person-2"] }
                  }
                }
                """)!;
            var validation = await manager.ValidateAsync(item);
            Assert.Equal(multiple, validation.Succeeded);
            if (!multiple)
            {
                Assert.Contains(validation.Errors, error => error.ErrorMessage.Contains("cannot contain multiple items", StringComparison.Ordinal));
            }
        });
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

    private static T Invoke<T>(object target, string methodName, params object[] arguments)
    {
        var method = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length);

        return (T)method.Invoke(target, arguments)!;
    }

    private static async Task<T> InvokeAsync<T>(object target, string methodName, params object[] arguments)
    {
        var method = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length);
        var task = (Task)method.Invoke(target, arguments)!;
        await task;

        return (T)task.GetType().GetProperty("Result")!.GetValue(task)!;
    }

    private sealed class DictionarySchema
    {
        public Dictionary<string, string[]> Values { get; set; } = [];
    }
}
