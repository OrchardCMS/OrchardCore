using System;

namespace OrchardCore.Shortcodes
{
    public class ShortcodeDescriptor
    {
        public string Name { get; set; }
        private string _defaultShortcode;
        public string DefaultShortcode
        {
            get
            {
                if (String.IsNullOrEmpty(_defaultShortcode))
                {
                    return '[' + Name + ']';
                }
                else
                {
                    return _defaultShortcode;
                }
            }
            set
            {
                _defaultShortcode = value;
            }
        }

        public string DefaultContent { get; set; }
        public string Hint { get; set; }
        public string[] Categories { get; set; } = Array.Empty<string>();
    }
}
