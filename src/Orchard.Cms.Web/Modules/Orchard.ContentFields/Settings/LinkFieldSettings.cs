namespace Orchard.ContentFields.Settings
{
    public class LinkFieldSettings
    {
        public string Hint { get; set; }
        public string Label { get; set; }
        public bool Required { get; set; }
        public TargetMode TargetMode { get; set; }
        public LinkTextMode LinkTextMode { get; set; }
        public string StaticText { get; set; }
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
        // some text can be entered or not, if not the url is used
        Optional,
        // some text must be entered
        Required,
        // the text is hard coded in the settings
        Static,
        // the url is used
        Url
    }
}