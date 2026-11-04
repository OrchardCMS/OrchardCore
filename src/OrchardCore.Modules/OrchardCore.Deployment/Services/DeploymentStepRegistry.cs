using System.Text.Json.Nodes;

namespace OrchardCore.Deployment.Services;

internal sealed class DeploymentStepRegistry
{
    private readonly Dictionary<string, IDeploymentStepFactory> _factories;
    private readonly Dictionary<string, IDeploymentStepDefinition> _definitions;

    public DeploymentStepRegistry(IEnumerable<IDeploymentStepFactory> factories, IEnumerable<IDeploymentStepDefinition> definitions)
    {
        _factories = factories.ToDictionary(factory => factory.Name, StringComparer.Ordinal);
        _definitions = definitions.ToDictionary(definition => definition.Type, StringComparer.Ordinal);
    }

    internal IReadOnlyList<DeploymentStepTypeDescriptor> List() => _factories.Keys.Order(StringComparer.Ordinal)
        .Select(type => new DeploymentStepTypeDescriptor { Type = type, CanConfigure = _definitions.ContainsKey(type) }).ToArray();

    internal bool IsAvailable(string type) => type is not null && _factories.ContainsKey(type);
    internal JsonObject GetSchema(string type) => IsAvailable(type) && _definitions.TryGetValue(type, out var definition)
        ? definition.GetSchema() : null;

    internal DeploymentStep Create(string type) => IsAvailable(type) && _definitions.ContainsKey(type) ? _factories[type].Create() : null;

    internal IDeploymentStepDefinition Definition(DeploymentStep step)
    {
        var type = Resolve(step);
        return _definitions.TryGetValue(type, out var definition) && _factories.TryGetValue(type, out var factory)
            && factory.Create().GetType() == step.GetType() ? definition : null;
    }

    internal string Resolve(DeploymentStep step) => DeploymentStepTypeResolver.Resolve(step, _factories);
}

/// <summary>Describes a registered deployment step without inspecting its persisted configuration.</summary>
public sealed class DeploymentStepTypeDescriptor
{
    /// <summary>Gets the factory identity accepted by recipes and typed step operations.</summary>
    public string Type { get; init; }
    /// <summary>Gets whether an explicit configuration contract is registered for the factory.</summary>
    public bool CanConfigure { get; init; }
}
