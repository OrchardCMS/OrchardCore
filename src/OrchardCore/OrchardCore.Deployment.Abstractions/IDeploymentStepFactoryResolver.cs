namespace OrchardCore.Deployment;

/// <summary>
/// Resolves all available deployment step factories, including auto-discovered ones
/// from recipe steps that support export.
/// </summary>
public interface IDeploymentStepFactoryResolver
{
    /// <summary>
    /// Gets all available deployment step factories, including both explicitly registered
    /// factories and auto-discovered recipe-step-based factories.
    /// </summary>
    IReadOnlyList<IDeploymentStepFactory> GetFactories();

    /// <summary>
    /// Gets a deployment step factory by name.
    /// </summary>
    /// <param name="name">The factory name.</param>
    /// <returns>The factory, or <c>null</c> if not found.</returns>
    IDeploymentStepFactory GetFactory(string name);
}
