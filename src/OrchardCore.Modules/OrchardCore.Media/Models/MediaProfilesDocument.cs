using System;
using System.Collections.Generic;
using Newtonsoft.Json;
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
        /// <summary>
        /// True if the object can't be used to update the database.
        /// </summary>
        [JsonIgnore]
        public bool IsReadonly { get; set; }

        //TODO if we do this as part of a processing command switcher we will need a guid profile version id.
        public string Hint { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
        public ResizeMode Mode { get; set; }
        public Format Format { get; set; }
        public int Quality { get; set; }
    }
}
