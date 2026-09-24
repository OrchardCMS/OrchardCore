using System.Text.Json.Serialization;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Deployment;

public abstract class DeploymentStep
{
    public string Id { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// The category of the step, used to group the steps of the picker. Set it in the step's parameterless
    /// constructor with <c>LocalizedString.Create("...")</c>.
    /// </summary>
    /// <remarks>
    /// The category is not translated. Translate it with
    /// <see cref="DeploymentStepExtensions.GetLocalizedCategory(DeploymentStep, IStringLocalizerFactory)"/>, which uses the
    /// step type as the context. It is never persisted: the value is always read from a fresh instance built by the
    /// step's factory. Persisting it would also break the plan, because <see cref="LocalizedString"/> has no
    /// constructor <see cref="System.Text.Json.JsonSerializer"/> can call.
    /// </remarks>
    [JsonIgnore]
    public LocalizedString Category { get; set; } = new(string.Empty, string.Empty);

    /// <summary>
    /// The title of the step, shown as the heading of its screens. Set it in the step's parameterless constructor
    /// with <c>LocalizedString.Create("...")</c>, alongside <see cref="Category"/>.
    /// </summary>
    /// <remarks>
    /// The title is not translated. Translate it with
    /// <see cref="DeploymentStepExtensions.GetLocalizedTitle(DeploymentStep, IStringLocalizerFactory)"/>, which uses the
    /// step type as the context. It is never persisted, for the reasons given on <see cref="Category"/>.
    /// </remarks>
    [JsonIgnore]
    public LocalizedString Title { get; set; }
}
