using System;
using System.Collections.Generic;
using OrchardCore.Data.Documents;
using OrchardCore.Media.Processing;

namespace OrchardCore.Media.Models
{
    public class MediaProfilesDocument : Document
    {
        public Dictionary<string, MediaProfile> MediaProfiles { get; } = new Dictionary<string, MediaProfile>(StringComparer.OrdinalIgnoreCase);
    }

    public class MediaProfile
    {
        public string Hint { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
        public ResizeMode Mode { get; set; }
        public Format Format { get; set; }
        public int Quality { get; set; }
        public string BackgroundColor { get; set; }
    }
}
