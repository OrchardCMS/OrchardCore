using OrchardCore.ContentManagement;

namespace OrchardCore.Alias.Models
{
    public class AliasPart : ContentPart
    {
        public const int MaxAliasLength = 735;

        public string Alias { get; set; }
    }
}
