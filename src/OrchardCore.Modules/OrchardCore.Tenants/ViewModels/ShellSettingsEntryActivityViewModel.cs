using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.Tenants.ViewModels;

/// <summary>
/// The activity of a tenant on the node rendering the tenants admin list.
/// </summary>
public class ShellSettingsEntryActivityViewModel
{
    /// <summary>
    /// The name of the current node.
    /// </summary>
    public string NodeName { get; set; }

    /// <summary>
    /// Whether the tenant has a request pipeline on the current node, which is built on its first request.
    /// </summary>
    public bool IsAwake { get; set; }

    /// <summary>
    /// The UTC date and time of the last request on the current node, if the tenant is awake.
    /// </summary>
    public DateTime? LastRequestUtc { get; set; }

    /// <summary>
    /// Updates this model from the shell context of the tenant on the current node, if any.
    /// </summary>
    public void Update(ShellContext shellContext, string nodeName)
    {
        NodeName = nodeName;
        IsAwake = shellContext.HasPipeline();
        LastRequestUtc = IsAwake ? shellContext.LastRequestTimeUtc : null;
    }
}
