namespace OrchardCore.Media.ViewModels;

public class MediaFieldSettingsViewModel
{
    public string Hint { get; set; }

    public bool Required { get; set; }

    public bool Multiple { get; set; }

    public bool AllowMediaText { get; set; }

    public bool AllowAnchors { get; set; }

    public bool AllowAllDefaultMediaTypes { get; set; }

    public MediaTypeViewModel[] MediaTypes { get; set; }
}
