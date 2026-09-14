using Microsoft.Extensions.Localization;

namespace OrchardCore.Deployment;

public abstract class DeploymentStep
{
    public string Id { get; set; }

    public string Name { get; set; }

    public LocalizedString Category { get; set; } = new(string.Empty, string.Empty);

    /// <summary>
    /// The localized display name of the step, shown as the title of its screens. Set it in the step's constructor,
    /// alongside <see cref="Category"/>.
    /// </summary>
    public LocalizedString DisplayName { get; set; }
}
