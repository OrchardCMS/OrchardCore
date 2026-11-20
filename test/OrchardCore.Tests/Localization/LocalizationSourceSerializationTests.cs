using System.Text.Json;
using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Extensions;
using OrchardCore.Json;
using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization;

public class LocalizationSourceSerializationTests
{
    [Fact]
    public void Serialize_DeploymentPlanWithStepWithJsonIgnore_SavesAndReadsBack()
    {
        // The options YesSql saves the documents with.
        using var provider = new ServiceCollection()
            .AddJsonDerivedTypeInfo<StepWithJsonIgnore, DeploymentStep>()
            .AddTransient<IConfigureOptions<DocumentJsonSerializerOptions>, DocumentJsonSerializerOptionsConfiguration>()
            .BuildServiceProvider();

        var documentOptions = provider.GetRequiredService<IOptions<DocumentJsonSerializerOptions>>().Value;

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
    public void Types_LocalizationSourceInstanceProperties_AreMarkedWithJsonIgnore()
    {
        var types = GetTypes().ToList();

        Assert.NotEmpty(types);

        // A source is a static declaration owned by code, never data: it names a Type, which can't be serialized, and
        // a type can't be created safely from a name read from a document. 'System.Text.Json' doesn't apply the
        // attribute of a base property to an override, so every override needs its own. Static properties are not
        // serialized.
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

    public sealed class StepWithJsonIgnore : DeploymentStep
    {
        private static readonly LocalizationSource s_category = LocalizationSource.Create<StepWithJsonIgnore>("Hello");

        public override string Name => "StepWithJsonIgnore";

        [JsonIgnore]
        public override LocalizationSource Category => s_category;
    }
}
