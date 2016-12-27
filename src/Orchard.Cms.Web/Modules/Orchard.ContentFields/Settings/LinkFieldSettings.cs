namespace Orchard.ContentFields.Settings
{
    public class LinkFieldSettings
    {
        public string Hint { get; set; }
        public string HintLinkText { get; set; }
        public bool Required { get; set; }
        public TargetMode TargetMode { get; set; }
        public LinkTextMode LinkTextMode { get; set; }
        public string UrlPlaceholder { get; set; }
        public string TextPlaceholder { get; set; }
        public string DefaultValue { get; set; }
        public string TextDefaultValue { get; set; }

        public LinkFieldSettings()
        {
            TargetMode = TargetMode.None;
            LinkTextMode = LinkTextMode.Optional;
        }
    }

    public enum TargetMode
    {
        None,
        NewWindow,
        Parent,
        Top,
        UserChoice
    }

    public enum LinkTextMode
    {
        // Some text can be entered or not, if not the url is used
        Optional,
        // Some text must be entered
        Required,
        // Use the default text value defined in the settings
        Static,
        // Use the url
        Url
    }
}