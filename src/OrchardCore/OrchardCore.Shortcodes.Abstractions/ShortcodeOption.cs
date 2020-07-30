using System;

namespace OrchardCore.Shortcodes
{
    public class ShortcodeOption
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
        /// The localizable hint text for the shortcode.
        /// </summary>
        public Func<IServiceProvider, string> Hint { get; set; }
        /// <summary>
        /// The non localized HTML usage description for the shortcode.
        /// </summary>
        public string Usage { get; set; }
        /// <summary>
        /// The localizable categories for the shortcode.
        /// </summary>
        public Func<IServiceProvider, string[]> Categories { get; set; }
    }
}
