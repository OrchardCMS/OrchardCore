using OrchardCore.DisplayManagement;

namespace OrchardCore.Admin.Models;

/// <summary>
/// Model for admin navigation data with source-generated Arguments provider.
/// </summary>
[GenerateArgumentsProvider]
public partial class AdminNavigationShapeData
{
    public string Title { get; set; }
    public string Url { get; set; }
    public string IconClass { get; set; }
    public int Position { get; set; }
    public bool IsActive { get; set; }
}
