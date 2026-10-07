using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Variables;

public sealed class WorkflowVariablesTests
{
    private readonly Dictionary<string, object> _properties = [];

    [Fact]
    public void Set_DeclaredVariable_StoresTheConvertedValueUnderItsDeclaredName()
    {
        var variables = Create(new WorkflowVariableDefinition { Name = "count", TypeName = "number" });

        variables["COUNT"] = "41";

        Assert.Equal(41d, _properties["count"]);
        Assert.Equal(41d, variables["count"]);
        Assert.True(variables.IsDeclared("Count"));
    }

    [Fact]
    public void Get_ValueStoredThroughTheProperties_IsConvertedOnRead()
    {
        var variables = Create(new WorkflowVariableDefinition { Name = "count", TypeName = "number" });

        // After a save and reload, a whole number comes back as an int.
        _properties["count"] = 41;

        Assert.Equal(41d, variables.Get("count"));
    }

    [Fact]
    public void Get_ValueThatDoesNotConvert_IsReturnedAsItIs()
    {
        var variables = Create(new WorkflowVariableDefinition { Name = "count", TypeName = "number" });
        _properties["count"] = "many";

        Assert.Equal("many", variables.Get("count"));
    }

    [Fact]
    public void Set_ValueThatDoesNotConvert_Throws()
    {
        var variables = Create(new WorkflowVariableDefinition { Name = "count", TypeName = "number" });

        var exception = Assert.Throws<WorkflowVariableException>(() => variables.Set("count", "many"));

        Assert.Equal("count", exception.VariableName);
        Assert.False(_properties.ContainsKey("count"));
    }

    [Fact]
    public void SetAndGet_UndeclaredName_UseThePropertiesAsTheyAre()
    {
        var variables = Create();
        var value = new { A = 1 };

        variables.Set("anything", value);

        Assert.Same(value, _properties["anything"]);
        Assert.Same(value, variables.Get("anything"));
        Assert.Null(variables.Get("missing"));
        Assert.False(variables.TryGetValue("missing", out _));
    }

    [Fact]
    public void Set_TypeThatIsNotRegistered_KeepsTheValue()
    {
        var variables = Create(new WorkflowVariableDefinition { Name = "user", TypeName = "user" });

        variables.Set("user", "admin");

        Assert.Equal("admin", variables.Get("user"));
    }

    [Fact]
    public void ApplyDefaults_SetsOnlyVariablesWithoutAValue()
    {
        var variables = Create(
            new WorkflowVariableDefinition { Name = "greeting", TypeName = "string", DefaultValue = "Hello" },
            new WorkflowVariableDefinition { Name = "count", TypeName = "number", DefaultValue = 1 },
            new WorkflowVariableDefinition { Name = "items", TypeName = "array", DefaultValue = new JsonArray("a") },
            new WorkflowVariableDefinition { Name = "none", TypeName = "string" });
        _properties["count"] = 5d;

        variables.ApplyDefaults();

        Assert.Equal("Hello", _properties["greeting"]);
        Assert.Equal(5d, _properties["count"]);
        Assert.Equal(new List<object> { "a" }, _properties["items"]);
        Assert.False(_properties.ContainsKey("none"));
        Assert.Equal(["greeting", "count", "items", "none"], variables.ToDictionary().Keys);
    }

    [Fact]
    public void ContentItemType_ContentAndOtherValues_KeepsOnlyContentItems()
    {
        var type = TestVariableTypes.CreateProvider().Get("contentItem");
        var contentItem = new ContentItem { ContentItemId = "item-1" };

        Assert.True(type.TryCoerce(contentItem, out var result));
        Assert.Same(contentItem, result);
        Assert.False(type.TryCoerce("item-1", out _));
        Assert.Equal("none", type.Editor);
    }

    [Fact]
    public async Task ContentItemVariable_SaveAndReload_IsLoadedAgain()
    {
        var contentItem = new ContentItem { ContentItemId = "item-1", ContentType = "Article" };
        var contentManager = new Mock<IContentManager>();
        contentManager.Setup(x => x.GetAsync("item-1", It.IsAny<VersionOptions>())).ReturnsAsync(contentItem);
        var serializer = new ContentItemSerializer(contentManager.Object);

        // What PersistAsync stores, and how the engine reads it back.
        var save = new SerializeWorkflowValueContext(contentItem);
        await serializer.SerializeValueAsync(save);
        var stored = Assert.IsType<JsonObject>(save.Output);
        var load = new SerializeWorkflowValueContext(stored.Deserialize<Dictionary<string, object>>(JOptions.Default));
        await serializer.DeserializeValueAsync(load);
        _properties["article"] = load.Output;

        var variables = Create(new WorkflowVariableDefinition { Name = "article", TypeName = "contentItem" });

        Assert.Same(contentItem, variables.Get("article"));
    }

    [Fact]
    public void ApplyInputs_InputValues_SetTheInputVariablesOnly()
    {
        var variables = Create(
            new WorkflowVariableDefinition { Name = "amount", TypeName = "number", IsInput = true },
            new WorkflowVariableDefinition { Name = "approved", TypeName = "boolean", IsInput = true },
            new WorkflowVariableDefinition { Name = "reason", TypeName = "string" });

        var failed = variables.ApplyInputs(new Dictionary<string, object>
        {
            ["AMOUNT"] = "12.5",
            ["approved"] = "maybe",
            ["reason"] = "Not an input",
        });

        Assert.Equal(12.5d, _properties["amount"]);
        Assert.Equal(["approved"], failed);
        Assert.False(_properties.ContainsKey("approved"));
        Assert.False(_properties.ContainsKey("reason"));
        Assert.Empty(variables.ApplyInputs(null));
    }

    [Fact]
    public void GetOutputs_OutputVariables_ReturnsThoseWithAValue()
    {
        var variables = Create(
            new WorkflowVariableDefinition { Name = "approved", TypeName = "boolean", IsOutput = true },
            new WorkflowVariableDefinition { Name = "comment", TypeName = "string", IsOutput = true },
            new WorkflowVariableDefinition { Name = "internal", TypeName = "string" });
        variables.Set("approved", "true");
        variables.Set("internal", "Not an output");

        var outputs = variables.GetOutputs();

        var output = Assert.Single(outputs);
        Assert.Equal("approved", output.Key);
        Assert.Equal(true, output.Value);
    }

    [Fact]
    public void Clone_InputAndOutput_AreCopied()
    {
        var clone = new WorkflowVariableDefinition { Name = "amount", TypeName = "number", IsInput = true, IsOutput = true }.Clone();

        Assert.True(clone.IsInput);
        Assert.True(clone.IsOutput);
    }

    private WorkflowVariables Create(params WorkflowVariableDefinition[] definitions)
    {
        var provider = TestVariableTypes.CreateProvider();

        return new WorkflowVariables(_properties, definitions, provider.Get);
    }
}
