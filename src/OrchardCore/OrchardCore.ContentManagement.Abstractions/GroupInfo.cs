using Microsoft.Extensions.Localization;

namespace OrchardCore.ContentManagement
{
    /// <summary>
    /// Describes a group and its position relative to other groups.
    /// It's used in <see cref="ContentItemMetadata"/> to describe the potential
    /// groups that a content item can be displayed in.
    /// </summary>
    public class GroupInfo
    {
        public GroupInfo(LocalizedString name)
        {
            Id = name.Name;
            Name = name;
        }

        public string Id { get; set; }
        public LocalizedString Name { get; set; }
        public string Position { get; set; }
    }
}
