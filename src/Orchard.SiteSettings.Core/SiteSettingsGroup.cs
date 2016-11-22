using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace Orchard.SiteSettings
{
    /// <summary>
    /// Holds the list of available site settings group ids.
    /// </summary>
    public class SiteSettingsGroupProvider
    {
        public void Add(string groupId, LocalizedString name)
        {
            Groups[groupId] = name;
        }

        public Dictionary<string, LocalizedString> Groups { get; } = new Dictionary<string, LocalizedString>();
    }
}
