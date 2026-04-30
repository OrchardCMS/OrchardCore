using Microsoft.Extensions.Localization;

namespace OrchardCore.Deployment;

public abstract class DeploymentStep
{
    public string Id { get; set; }

    public string Name { get; set; }

    public LocalizedString Category { get; set; } = new(string.Empty, string.Empty);
}
