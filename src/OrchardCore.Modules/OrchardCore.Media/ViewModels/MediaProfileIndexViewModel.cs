using System.Collections.Generic;
using OrchardCore.Media.Models;

namespace OrchardCore.Media.ViewModels
{
    public class MediaProfileIndexViewModel
    {
        public IList<MediaProfileEntry> MediaProfiles { get; set; }
        public dynamic Pager { get; set; }
    }

    public class MediaProfileEntry
    {
        public string Name { get; set; }
        public MediaProfile MediaProfile { get; set; }
        public bool IsChecked { get; set; }
    }
}
