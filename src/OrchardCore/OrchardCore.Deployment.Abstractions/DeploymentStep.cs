using System.Text.Json.Serialization;
using OrchardCore.Localization;

namespace OrchardCore.Deployment;

public abstract class DeploymentStep
{
    private static readonly LocalizationSource s_emptyCategory = new(string.Empty);

    public string Id { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// The category of the step, used to group the steps of the picker. Set it in the step's parameterless
    /// constructor with <c>new LocalizationSource("...", typeof(MyDeploymentStep))</c>.
    /// </summary>
    /// <remarks>
    /// The category is translated at display time using its source type as the context. It is never persisted:
    /// the value is always read from a fresh instance built by the step's factory.
    /// </remarks>
    [JsonIgnore]
    public LocalizationSource Category { get; set; } = s_emptyCategory;

    /// <summary>
    /// The title of the step, shown as the heading of its screens. Set it in the step's parameterless constructor
    /// with <c>new LocalizationSource("...", typeof(MyDeploymentStep))</c>, alongside <see cref="Category"/>.
    /// </summary>
    /// <remarks>
    /// The title is translated at display time using its source type as the context. It is never persisted,
    /// for the reasons given on <see cref="Category"/>.
    /// </remarks>
    [JsonIgnore]
    public LocalizationSource Title { get; set; }
}
