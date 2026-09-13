namespace OrchardCore.Deployment;

/// <summary>Resolves the factory identity used by deployment recipes and management contracts.</summary>
public static class DeploymentStepTypeResolver
{
    /// <summary>Uses a registered step name for generic settings steps, otherwise its concrete type name.</summary>
    public static string Resolve(DeploymentStep step, IReadOnlyDictionary<string, IDeploymentStepFactory> factories)
    {
        ArgumentNullException.ThrowIfNull(step);
        ArgumentNullException.ThrowIfNull(factories);
        return step.Name is not null && factories.TryGetValue(step.Name, out var factory)
            ? factory.Name : step.GetType().Name;
    }
}
