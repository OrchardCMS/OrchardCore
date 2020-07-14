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
        private readonly Dictionary<string, ShortcodeDescriptor> _shortcodes = new Dictionary<string, ShortcodeDescriptor>();

        public IReadOnlyDictionary<string, ShortcodeDescriptor> Shortcodes => _shortcodes;

        private readonly Dictionary<string, ShortcodeDelegate> _shortcodeDelegates = new Dictionary<string, ShortcodeDelegate>();

        public IReadOnlyDictionary<string, ShortcodeDelegate> ShortcodeDelegates => _shortcodeDelegates;

        internal ShortcodeOptions AddShortcode(string name, Action<ShortcodeDescriptor> describe)
        {
            var descriptor = new ShortcodeDescriptor { Name = name };
            describe?.Invoke(descriptor);
            _shortcodes[name] = descriptor;

            return this;
        }

        internal ShortcodeOptions AddShortcodeDelegate(string name, ShortcodeDelegate shortcode, Action<ShortcodeDescriptor> describe)
        {
            var descriptor = new ShortcodeDescriptor { Name = name };
            describe?.Invoke(descriptor);
            _shortcodes[name] = descriptor;
            _shortcodeDelegates[name] = shortcode;

            return this;
        }
    }
}
