using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using OrchardVNext.Utility.Extensions;

namespace OrchardVNext.ContentManagement.MetaData.Models {
    public class ContentTypeDefinition {
        public ContentTypeDefinition(string name, string displayName, IEnumerable<ContentTypePartDefinition> parts, SettingsDictionary settings) {
            Name = name;
            DisplayName = displayName;
            Parts = parts.ToReadOnlyCollection();
            Settings = settings;
        }

        public ContentTypeDefinition(string name, string displayName) {
            Name = name;
            DisplayName = displayName;
            Parts = Enumerable.Empty<ContentTypePartDefinition>();
            Settings = new SettingsDictionary();
        }

        [StringLength(128)]
        public string Name { get; private set; }
        [Required, StringLength(1024)]
        public string DisplayName { get; private set; }
        public IEnumerable<ContentTypePartDefinition> Parts { get; private set; }
        public SettingsDictionary Settings { get; private set; }
    }
}
