using System.Text.Json;
using System.Text.Json.Serialization;
using OrchardCore.Json;

namespace OrchardCore.Tests.Serializers;

public class ResilientPolymorphicJsonConverterTests
{
    private static JsonSerializerOptions CreateOptions(Action<JsonDerivedTypesOptions> configure)
    {
        var derivedTypesOptions = new JsonDerivedTypesOptions();
        configure(derivedTypesOptions);

        var options = new JsonSerializerOptions(JOptions.Base);
        options.Converters.Add(new ResilientPolymorphicJsonConverterFactory(derivedTypesOptions));
        options.TypeInfoResolverChain.Add(new PolymorphicJsonTypeInfoResolver(derivedTypesOptions));

        return options;
    }

    private static void RegisterDerivedType<TDerived, TBase>(JsonDerivedTypesOptions options)
        where TDerived : class
        where TBase : class
    {
        if (!options.DerivedTypes.TryGetValue(typeof(TBase), out var derivedTypes))
        {
            derivedTypes = [];
            options.DerivedTypes[typeof(TBase)] = derivedTypes;
        }

        derivedTypes.Add(new JsonDerivedTypeInfo<TDerived, TBase>());
    }

    [Fact]
    public void Deserialize_WithKnownDiscriminator_ReturnsCorrectDerivedType()
    {
        var options = CreateOptions(o =>
        {
            RegisterDerivedType<ConcreteStepA, AbstractBaseStep>(o);
            RegisterDerivedType<ConcreteStepB, AbstractBaseStep>(o);
        });

        var discriminator = JsonDerivedTypeInfo<ConcreteStepA, AbstractBaseStep>.CreateTypeDiscriminator<ConcreteStepA>();
        var json = $$"""{"$type":"{{discriminator}}","Name":"StepA","ValueA":"hello"}""";

        var result = JsonSerializer.Deserialize<AbstractBaseStep>(json, options);

        Assert.NotNull(result);
        var stepA = Assert.IsType<ConcreteStepA>(result);
        Assert.Equal("StepA", stepA.Name);
        Assert.Equal("hello", stepA.ValueA);
    }

    [Fact]
    public void Deserialize_WithUnknownDiscriminator_WithoutFallback_ReturnsNull()
    {
        // No fallback registered — unrecognized discriminator returns null.
        var options = CreateOptions(RegisterDerivedType<ConcreteStepA, AbstractBaseStep>);

        var unknownDiscriminator = JsonDerivedTypeInfo<ConcreteStepB, AbstractBaseStep>.CreateTypeDiscriminator<ConcreteStepB>();
        var json = $$"""{"$type":"{{unknownDiscriminator}}","Name":"StepB","ValueB":42}""";

        var result = JsonSerializer.Deserialize<AbstractBaseStep>(json, options);

        Assert.Null(result);
    }

    [Fact]
    public void Deserialize_WithUnknownDiscriminator_WithFallback_ReturnsFallbackType()
    {
        var options = CreateOptions(o =>
        {
            RegisterDerivedType<ConcreteStepA, AbstractBaseStep>(o);
            o.FallbackTypes[typeof(AbstractBaseStep)] = typeof(UnknownStep);
        });

        var unknownDiscriminator = JsonDerivedTypeInfo<ConcreteStepB, AbstractBaseStep>.CreateTypeDiscriminator<ConcreteStepB>();
        var json = $$"""{"$type":"{{unknownDiscriminator}}","Name":"StepB","ValueB":42}""";

        var result = JsonSerializer.Deserialize<AbstractBaseStep>(json, options);

        Assert.NotNull(result);
        var unknown = Assert.IsType<UnknownStep>(result);
        Assert.Equal("StepB", unknown.Name);
        Assert.Equal(unknownDiscriminator, unknown.TypeDiscriminator);
        Assert.Equal(JsonValueKind.Object, unknown.RawData.ValueKind);
    }

    [Fact]
    public void Deserialize_WithMissingDiscriminator_WithFallback_ReturnsFallbackType()
    {
        var options = CreateOptions(o =>
        {
            RegisterDerivedType<ConcreteStepA, AbstractBaseStep>(o);
            o.FallbackTypes[typeof(AbstractBaseStep)] = typeof(UnknownStep);
        });

        var json = """{"Name":"NoType"}""";

        var result = JsonSerializer.Deserialize<AbstractBaseStep>(json, options);

        Assert.NotNull(result);
        var unknown = Assert.IsType<UnknownStep>(result);
        Assert.Equal("NoType", unknown.Name);
        Assert.Null(unknown.TypeDiscriminator);
    }

    [Fact]
    public void Deserialize_CollectionWithMixedDiscriminators_FallbackForUnknown()
    {
        var options = CreateOptions(o =>
        {
            RegisterDerivedType<ConcreteStepA, AbstractBaseStep>(o);
            o.FallbackTypes[typeof(AbstractBaseStep)] = typeof(UnknownStep);
        });

        var knownDiscriminator = JsonDerivedTypeInfo<ConcreteStepA, AbstractBaseStep>.CreateTypeDiscriminator<ConcreteStepA>();
        var unknownDiscriminator = JsonDerivedTypeInfo<ConcreteStepB, AbstractBaseStep>.CreateTypeDiscriminator<ConcreteStepB>();

        var json = $$"""
            {
                "Name":"TestPlan",
                "Steps":[
                    {"$type":"{{knownDiscriminator}}","Name":"Known","ValueA":"a"},
                    {"$type":"{{unknownDiscriminator}}","Name":"Unknown","ValueB":1}
                ]
            }
            """;

        var result = JsonSerializer.Deserialize<StepContainer>(json, options);

        Assert.NotNull(result);
        Assert.Equal("TestPlan", result.Name);
        Assert.Equal(2, result.Steps.Count);

        // The known step should be deserialized correctly.
        var knownStep = Assert.IsType<ConcreteStepA>(result.Steps[0]);
        Assert.Equal("Known", knownStep.Name);
        Assert.Equal("a", knownStep.ValueA);

        // The unknown step should be deserialized as the fallback type.
        var unknownStep = Assert.IsType<UnknownStep>(result.Steps[1]);
        Assert.Equal("Unknown", unknownStep.Name);
        Assert.Equal(unknownDiscriminator, unknownStep.TypeDiscriminator);
    }

    [Fact]
    public void Serialize_WithKnownDerivedType_WritesDiscriminator()
    {
        var options = CreateOptions(o =>
        {
            RegisterDerivedType<ConcreteStepA, AbstractBaseStep>(o);
            RegisterDerivedType<ConcreteStepB, AbstractBaseStep>(o);
        });

        AbstractBaseStep step = new ConcreteStepA { Name = "StepA", ValueA = "hello" };

        var json = JsonSerializer.Serialize(step, options);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var discriminator = JsonDerivedTypeInfo<ConcreteStepA, AbstractBaseStep>.CreateTypeDiscriminator<ConcreteStepA>();
        Assert.True(root.TryGetProperty("$type", out var typeProp));
        Assert.Equal(discriminator, typeProp.GetString());
        Assert.Equal("StepA", root.GetProperty("Name").GetString());
        Assert.Equal("hello", root.GetProperty("ValueA").GetString());
    }

    [Fact]
    public void Serialize_UnknownPlaceholder_PreservesOriginalJson()
    {
        var options = CreateOptions(o =>
        {
            RegisterDerivedType<ConcreteStepA, AbstractBaseStep>(o);
            o.FallbackTypes[typeof(AbstractBaseStep)] = typeof(UnknownStep);
        });

        var unknownDiscriminator = JsonDerivedTypeInfo<ConcreteStepB, AbstractBaseStep>.CreateTypeDiscriminator<ConcreteStepB>();
        var originalJson = $$"""{"$type":"{{unknownDiscriminator}}","Name":"StepB","ValueB":42}""";

        // Deserialize with unknown discriminator → fallback type.
        var step = JsonSerializer.Deserialize<AbstractBaseStep>(originalJson, options);
        Assert.IsType<UnknownStep>(step);

        // Re-serialize — should produce the original JSON with discriminator intact.
        var reserializedJson = JsonSerializer.Serialize(step, options);

        using var doc = JsonDocument.Parse(reserializedJson);
        var root = doc.RootElement;

        Assert.Equal(unknownDiscriminator, root.GetProperty("$type").GetString());
        Assert.Equal("StepB", root.GetProperty("Name").GetString());
        Assert.Equal(42, root.GetProperty("ValueB").GetInt32());
    }

    [Fact]
    public void Serialize_ThenDeserialize_RoundTrips()
    {
        var options = CreateOptions(o =>
        {
            RegisterDerivedType<ConcreteStepA, AbstractBaseStep>(o);
            RegisterDerivedType<ConcreteStepB, AbstractBaseStep>(o);
        });

        var container = new StepContainer
        {
            Name = "Plan",
            Steps =
            [
                new ConcreteStepA { Name = "A", ValueA = "val" },
                new ConcreteStepB { Name = "B", ValueB = 99 },
            ],
        };

        var json = JsonSerializer.Serialize(container, options);
        var result = JsonSerializer.Deserialize<StepContainer>(json, options);

        Assert.NotNull(result);
        Assert.Equal("Plan", result.Name);
        Assert.Equal(2, result.Steps.Count);

        var stepA = Assert.IsType<ConcreteStepA>(result.Steps[0]);
        Assert.Equal("A", stepA.Name);
        Assert.Equal("val", stepA.ValueA);

        var stepB = Assert.IsType<ConcreteStepB>(result.Steps[1]);
        Assert.Equal("B", stepB.Name);
        Assert.Equal(99, stepB.ValueB);
    }

    [Fact]
    public void Deserialize_NullElement_ReturnsNull()
    {
        var options = CreateOptions(RegisterDerivedType<ConcreteStepA, AbstractBaseStep>);

        var json = "null";

        var result = JsonSerializer.Deserialize<AbstractBaseStep>(json, options);

        Assert.Null(result);
    }

    // Test types.

    public abstract class AbstractBaseStep
    {
        public string Name { get; set; }
    }

    public class ConcreteStepA : AbstractBaseStep
    {
        public string ValueA { get; set; }
    }

    public class ConcreteStepB : AbstractBaseStep
    {
        public int ValueB { get; set; }
    }

    public class UnknownStep : AbstractBaseStep, IUnknownTypePlaceholder
    {
        public string TypeDiscriminator { get; set; }
        public JsonElement RawData { get; set; }
    }

    public class StepContainer
    {
        public string Name { get; set; }
        public List<AbstractBaseStep> Steps { get; set; } = [];
    }
}
