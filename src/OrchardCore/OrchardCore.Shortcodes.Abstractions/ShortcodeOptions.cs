using System;
using System.Collections.Generic;
using Shortcodes;

namespace OrchardCore.Shortcodes
{
    /// <summary>
    /// Provides a way to register shortcodes for discovery.
    /// </summary>
    public class ShortcodeOptions
    {
        private readonly Dictionary<string, ShortcodeOption> _shortcodes = new();

        public IReadOnlyDictionary<string, ShortcodeOption> Shortcodes => _shortcodes;

        private readonly Dictionary<string, ShortcodeDelegate> _shortcodeDelegates = new();

        public IReadOnlyDictionary<string, ShortcodeDelegate> ShortcodeDelegates => _shortcodeDelegates;

        internal ShortcodeOptions AddShortcode(string name, Action<ShortcodeOption> describe)
        {
            var option = new ShortcodeOption { Name = name };
            describe?.Invoke(option);
            _shortcodes[name] = option;

            return this;
        }

        internal ShortcodeOptions AddShortcodeDelegate(string name, ShortcodeDelegate shortcode, Action<ShortcodeOption> describe)
        {
            var option = new ShortcodeOption { Name = name };
            describe?.Invoke(option);
            _shortcodes[name] = option;
            _shortcodeDelegates[name] = shortcode;

            return this;
        }
    }
}
