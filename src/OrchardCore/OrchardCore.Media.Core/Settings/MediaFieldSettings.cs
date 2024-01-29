using System;
using System.ComponentModel;

namespace OrchardCore.Media.Settings
{
    public class MediaFieldSettings
    {
        public string Hint { get; set; }

        public bool Required { get; set; }

        [DefaultValue(true)]
        public bool Multiple { get; set; } = true;

        [DefaultValue(true)]
        public bool AllowMediaText { get; set; } = true;

        public bool AllowAnchors { get; set; }

        public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
    }
}
