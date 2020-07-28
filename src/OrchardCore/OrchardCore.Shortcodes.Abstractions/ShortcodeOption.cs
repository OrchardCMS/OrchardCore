using System;

namespace OrchardCore.Shortcodes
{
    public class ShortcodeOption
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

        public Func<IServiceProvider, string> Hint { get; set; }
        public string Usage { get; set; }
        public Func<IServiceProvider, string[]> Categories { get; set; }
    }
}
