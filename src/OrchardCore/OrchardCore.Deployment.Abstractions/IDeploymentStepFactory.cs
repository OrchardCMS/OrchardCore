using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Localization;

namespace OrchardCore.Deployment;

public interface IDeploymentStepFactory
{
    string Name { get; }

    /// <summary>
    /// Gets the category of the steps the factory creates, to localize with <c>IStringLocalizerFactory.Localize</c>.
    /// </summary>
    LocalizationSource Category { get; }

    /// <summary>
    /// Gets the title of the steps the factory creates, or <see langword="null"/> when they have none,
    /// to localize with <c>IStringLocalizerFactory.Localize</c>.
    /// </summary>
    LocalizationSource Title { get; }

    DeploymentStep Create();
}

public class DeploymentStepFactory<TStep> : IDeploymentStepFactory where TStep : DeploymentStep, IDeploymentStep
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly string s_typeName = typeof(TStep).Name;

    public DeploymentStepFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public string Name => s_typeName;

    [JsonIgnore]
    public LocalizationSource Category => TStep.Category;

    [JsonIgnore]
    public LocalizationSource Title => TStep.Title;

    public DeploymentStep Create()
    {
        return ActivatorUtilities.CreateInstance<TStep>(_serviceProvider);
    }
}
