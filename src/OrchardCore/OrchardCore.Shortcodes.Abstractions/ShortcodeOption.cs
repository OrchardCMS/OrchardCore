using System;

namespace OrchardCore.Shortcodes
{
    public class ShortcodeOption
    {
        private string _defaultValue;

        /// <summary>
        /// The shortcode name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The default value of the shortcode when selected.
        /// </summary>
        public string DefaultValue
        {
            get
            {
                if (String.IsNullOrEmpty(_defaultValue))
                {
                    return '[' + Name + ']';
                }
                else
                {
                    return _defaultValue;
                }
            }
            set
            {
                _defaultValue = value;
            }
        }

        /// <summary>
        /// The hint text for the shortcode.
        /// </summary>
        public string Hint { get; set; }

        /// <summary>
        /// The HTML usage description for the shortcode.
        /// </summary>
        public string Usage { get; set; }

        /// <summary>
        /// The categories for the shortcode.
        /// </summary>
        public string[] Categories { get; set; }
    }
}
