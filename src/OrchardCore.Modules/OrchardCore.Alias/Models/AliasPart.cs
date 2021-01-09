using OrchardCore.ContentManagement;

namespace OrchardCore.Alias.Models
{
    public class AliasPart : ContentPart
    {
        // Maximum length that MySql can support in an index under utf8 collation.
        public const int MaxAliasLength = 700;

        public string Alias { get; set; }
    }
}
