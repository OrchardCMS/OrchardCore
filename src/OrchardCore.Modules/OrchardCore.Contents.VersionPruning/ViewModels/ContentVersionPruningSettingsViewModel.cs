namespace OrchardCore.Contents.VersionPruning.ViewModels;

public class ContentVersionPruningSettingsViewModel
{
    public int RetentionDays { get; set; }
    public int VersionsToKeep { get; set; }
    public string[] ContentTypes { get; set; } = [];

    public bool Disabled { get; set; }
    public DateTime? LastRunUtc { get; set; }
}
