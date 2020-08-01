using System;

namespace OrchardCore.Shortcodes
{
    public class ShortcodeDescriptor
    {
        /// <summary>
        /// The shortcode name.
        /// </summary>
        public string Name { get; set; }
        private string _returnShortcode;

        /// <summary>
        /// The return value of the shortcode when selected.
        /// </summary>
        public string ReturnShortcode
        {
            get
            {
                if (String.IsNullOrEmpty(_returnShortcode))
                {
                    return '[' + Name + ']';
                }
                else
                {
                    return _returnShortcode;
                }
            }
            set
            {
                _returnShortcode = value;
            }
        }

        /// <summary>
        /// The hint text for the shortcode.
        /// </sumary>
        public string Hint { get; set; }

        /// <summary>
        /// The HTML usage description for the shortcode.
        /// </summary>
        public string Usage { get; set; }

        /// <summary>
        /// The categories for the shortcode.
        /// </summary>
        public string[] Categories { get; set; } = Array.Empty<string>();
    }
}
