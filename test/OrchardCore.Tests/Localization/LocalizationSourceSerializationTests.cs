using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using OrchardCore.Deployment;
using OrchardCore.Extensions;
using OrchardCore.Json;
using OrchardCore.Json.Extensions;
using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization;

public class LocalizationSourceSerializationTests
{
    [Fact]
    public void Serialize_DeploymentPlanWithStepWithJsonIgnore_SavesAndReadsBack()
    {
        var documentOptions = CreateDocumentOptions<StepWithJsonIgnore>();

        var plan = new DeploymentPlan
        {
            Name = "Plan",
            DeploymentSteps = [new StepWithJsonIgnore { Id = "step1" }],
        };

        var json = JsonSerializer.Serialize(plan, documentOptions.SerializerOptions);
        var copy = JsonSerializer.Deserialize<DeploymentPlan>(json, documentOptions.SerializerOptions);

        Assert.DoesNotContain("Category", json, StringComparison.Ordinal);
        var step = Assert.IsType<StepWithJsonIgnore>(Assert.Single(copy.DeploymentSteps));
        Assert.Equal("step1", step.Id);
        Assert.Equal(typeof(StepWithJsonIgnore), step.Category.Type);
    }

    [Fact]
    public void Serialize_DeploymentPlanWithStepWithoutJsonIgnore_SavesAndReadsBack()
    {
        // The options YesSql saves the documents with. An override doesn't inherit the '[JsonIgnore]' of the base
        // property, but the document contract removes every LocalizationSource property.
        var documentOptions = CreateDocumentOptions<StepWithoutJsonIgnore>();

        var plan = new DeploymentPlan
        {
            Name = "Plan",
            DeploymentSteps = [new StepWithoutJsonIgnore { Id = "step1" }],
        };

        var json = JsonSerializer.Serialize(plan, documentOptions.SerializerOptions);
        var copy = JsonSerializer.Deserialize<DeploymentPlan>(json, documentOptions.SerializerOptions);

        Assert.DoesNotContain("Category", json, StringComparison.Ordinal);
        var step = Assert.IsType<StepWithoutJsonIgnore>(Assert.Single(copy.DeploymentSteps));
        Assert.Equal("step1", step.Id);
        Assert.Equal(typeof(StepWithoutJsonIgnore), step.Category.Type);
    }

    [Fact]
    public void Deserialize_DocumentWithStoredSource_IgnoresIt()
    {
        var documentOptions = CreateDocumentOptions<StepWithoutJsonIgnore>();

        var plan = new DeploymentPlan
        {
            Name = "Plan",
            DeploymentSteps = [new StepWithoutJsonIgnore { Id = "step1" }],
        };

        // A plan saved before the sources were excluded may still have them.
        var json = JsonSerializer.Serialize(plan, documentOptions.SerializerOptions)
            .Replace("\"Id\":\"step1\"", "\"Id\":\"step1\",\"Category\":\"Hello\"", StringComparison.Ordinal);

        Assert.Contains("\"Category\":\"Hello\"", json, StringComparison.Ordinal);

        var copy = JsonSerializer.Deserialize<DeploymentPlan>(json, documentOptions.SerializerOptions);

        var step = Assert.IsType<StepWithoutJsonIgnore>(Assert.Single(copy.DeploymentSteps));
        Assert.Equal("step1", step.Id);
        Assert.Equal(typeof(StepWithoutJsonIgnore), step.Category.Type);
    }

    [Fact]
    public void Serialize_JOptionsDefault_SkipsEveryPropertyHoldingASource()
    {
        var json = JsonSerializer.Serialize(new UnmarkedSourceHolder(), JOptions.Default);

        Assert.Equal("{\"Name\":\"holder\"}", json);
    }

    [Fact]
    public void Deserialize_JOptionsDefault_IgnoresEveryPropertyHoldingASource()
    {
        var holder = JsonSerializer.Deserialize<UnmarkedSourceHolder>(
            """{"Name":"other","Title":"Hello","Titles":["a"],"Map":{"a":"b"},"Items":["c"]}""",
            JOptions.Default);

        Assert.Equal("other", holder.Name);
        Assert.Equal(typeof(UnmarkedSourceHolder), holder.Title.Type);
        Assert.Single(holder.Titles);
        Assert.Equal(typeof(UnmarkedSourceHolder), holder.Titles[0].Type);
    }

    [Fact]
    public void Serialize_JOptionsCopies_SkipEveryPropertyHoldingASource()
    {
        Assert.Equal("{\"name\":\"holder\"}", JsonSerializer.Serialize(new UnmarkedSourceHolder(), JOptions.CamelCase));
        Assert.Equal("{\"Name\":\"holder\"}", JsonSerializer.Serialize(new UnmarkedSourceHolder(), JOptions.Indented).ReplaceLineEndings(string.Empty).Replace(" ", string.Empty, StringComparison.Ordinal));
    }

    [Fact]
    public void IgnoreLocalizationSources_NewOptions_SkipsEveryPropertyHoldingASource()
    {
        var options = new JsonSerializerOptions().IgnoreLocalizationSources();

        Assert.Equal("{\"Name\":\"holder\"}", JsonSerializer.Serialize(new UnmarkedSourceHolder(), options));
    }

    [Fact]
    public void IgnoreLocalizationSources_OptionsWithResolver_KeepsTheResolver()
    {
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers =
                {
                    typeInfo =>
                    {
                        // Renames a property to prove the existing resolver still runs.
                        foreach (var property in typeInfo.Properties.Where(property => property.Name == "Name"))
                        {
                            property.Name = "Renamed";
                        }
                    },
                },
            },
        }.IgnoreLocalizationSources();

        Assert.Equal("{\"Renamed\":\"holder\"}", JsonSerializer.Serialize(new UnmarkedSourceHolder(), options));
    }

    [Fact]
    public void Types_LocalizationSourceInstanceProperties_AreMarkedWithJsonIgnore()
    {
        var types = GetTypes().ToList();

        Assert.NotEmpty(types);

        // The serializers of Orchard Core remove these properties from their contract, but a property marked with
        // [JsonIgnore] also stays out of any other serializer, such as a custom JsonSerializerOptions, a source generator
        // or Newtonsoft. 'System.Text.Json' doesn't apply the attribute of a base property to an override, so every
        // override needs its own. Static properties are not serialized.
        var missing = types
            .SelectMany(type => type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(property => property.PropertyType == typeof(LocalizationSource))
                .Where(property => !property.IsDefined(typeof(JsonIgnoreAttribute), inherit: false))
                .Select(property => $"{type.FullName}.{property.Name}"))
            .Order()
            .ToList();

        Assert.Empty(missing);
    }

    private static DocumentJsonSerializerOptions CreateDocumentOptions<TStep>()
        where TStep : DeploymentStep, new()
    {
        using var provider = new ServiceCollection()
            .AddJsonDerivedTypeInfo<TStep, DeploymentStep>()
            .AddTransient<IConfigureOptions<DocumentJsonSerializerOptions>, DocumentJsonSerializerOptionsConfiguration>()
            .BuildServiceProvider();

        return provider.GetRequiredService<IOptions<DocumentJsonSerializerOptions>>().Value;
    }

    private static IEnumerable<Type> GetTypes()
    {
        foreach (var path in Directory.EnumerateFiles(AppContext.BaseDirectory, "OrchardCore.*.dll"))
        {
            // The test assemblies declare types of their own.
            if (Path.GetFileName(path).StartsWith("OrchardCore.Tests", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            Assembly assembly;

            try
            {
                assembly = Assembly.LoadFrom(path);
            }
            catch (BadImageFormatException)
            {
                continue;
            }

            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                types = exception.Types.Where(type => type != null).ToArray();
            }

            foreach (var type in types)
            {
                // The properties of an interface don't apply to its implementations.
                if (!type.IsInterface)
                {
                    yield return type;
                }
            }
        }
    }

    public sealed class UnmarkedSourceHolder
    {
        public string Name { get; set; } = "holder";

        public LocalizationSource Title { get; set; } = LocalizationSource.Create<UnmarkedSourceHolder>("Hello");

        public List<LocalizationSource> Titles { get; set; } = [LocalizationSource.Create<UnmarkedSourceHolder>("a")];

        public Dictionary<string, LocalizationSource> Map { get; set; } = new() { ["a"] = LocalizationSource.Create<UnmarkedSourceHolder>("b") };

        public LocalizationSource[] Items { get; set; } = [LocalizationSource.Create<UnmarkedSourceHolder>("c")];
    }

    public sealed class StepWithJsonIgnore : DeploymentStep
    {
        private static readonly LocalizationSource s_category = LocalizationSource.Create<StepWithJsonIgnore>("Hello");

        public override string Name => "StepWithJsonIgnore";

        [JsonIgnore]
        public override LocalizationSource Category => s_category;
    }

    public sealed class StepWithoutJsonIgnore : DeploymentStep
    {
        private static readonly LocalizationSource s_category = LocalizationSource.Create<StepWithoutJsonIgnore>("Hello");

        public override string Name => "StepWithoutJsonIgnore";

        public override LocalizationSource Category => s_category;
    }
}
