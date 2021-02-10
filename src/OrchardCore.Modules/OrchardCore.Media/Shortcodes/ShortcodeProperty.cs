namespace OrchardCore.Media.Shortcodes
{
    public class ShortcodeProperty
    {
        public string Name
        {
            get; set;
        }

        public string Mapping
        {
            get; set;
        }

        private ShortcodeProperty(string name) : this(name, name)
        {
        }

        private ShortcodeProperty(string name, string mapping)
        {
            Name = name;
            Mapping = mapping;
        }

        public static ShortcodeProperty Url
        {
            get
            {
                return new ShortcodeProperty("url", "href");
            }
        }
        public static ShortcodeProperty Save
        {
            get
            {
                return new ShortcodeProperty("save", "download");
            }
        }
        public static ShortcodeProperty Tooltip
        {
            get
            {
                return new ShortcodeProperty("tooltip", "title");
            }
        }
        public static ShortcodeProperty Class
        {
            get
            {
                return new ShortcodeProperty("class");
            }
        }
    }
}
