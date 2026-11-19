using System.Text.Json;
using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Extensions;
using OrchardCore.Json;
using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization;

public class LocalizationSourceJsonConverterTests
{
    [Fact]
    public void Serialize_WithDefaultOptions_WritesOnlyTheValue()
    {
        var json = JsonSerializer.Serialize(LocalizationSource.Create<LocalizationSourceJsonConverterTests>("Hello"));

        Assert.Equal("\"Hello\"", json);
    }

    [Fact]
    public void Deserialize_String_ReturnsSourceWithoutType()
    {
        var source = JsonSerializer.Deserialize<LocalizationSource>("\"Hello\"", JOptions.Default);

        Assert.Equal(LocalizationSource.Create("Hello"), source);
    }

    [Fact]
    public void Deserialize_Null_ReturnsNull()
    {
        Assert.Null(JsonSerializer.Deserialize<LocalizationSource>("null", JOptions.Default));
    }

    [Fact]
    public void Deserialize_Object_Throws()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<LocalizationSource>("""{"Value":"Hello"}""", JOptions.Default));
    }

    [Fact]
    public void Serialize_DeploymentPlanWithStepWithoutJsonIgnore_DoesNotThrow()
    {
        // The options YesSql saves the documents with. 'Category' has no '[JsonIgnore]', so it is written.
        using var provider = new ServiceCollection()
            .AddJsonDerivedTypeInfo<StepWithoutJsonIgnore, DeploymentStep>()
            .AddTransient<IConfigureOptions<DocumentJsonSerializerOptions>, DocumentJsonSerializerOptionsConfiguration>()
            .BuildServiceProvider();

        var documentOptions = provider.GetRequiredService<IOptions<DocumentJsonSerializerOptions>>().Value;

        var plan = new DeploymentPlan
        {
            Name = "Plan",
            DeploymentSteps = [new StepWithoutJsonIgnore { Id = "step1" }],
        };

        var json = JsonSerializer.Serialize(plan, documentOptions.SerializerOptions);
        var copy = JsonSerializer.Deserialize<DeploymentPlan>(json, documentOptions.SerializerOptions);

        Assert.Contains("\"Category\":\"Hello\"", json, StringComparison.Ordinal);
        var step = Assert.IsType<StepWithoutJsonIgnore>(Assert.Single(copy.DeploymentSteps));
        Assert.Equal("step1", step.Id);
        Assert.Equal(typeof(StepWithoutJsonIgnore), step.Category.Type);
    }

    public sealed class StepWithoutJsonIgnore : DeploymentStep
    {
        private static readonly LocalizationSource s_category = LocalizationSource.Create<StepWithoutJsonIgnore>("Hello");

        public override string Name => "StepWithoutJsonIgnore";

        public override LocalizationSource Category => s_category;
    }
}
