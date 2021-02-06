using OrchardCore.ContentManagement;

namespace OrchardCore.Alias.Models
{
    public class AliasPart : ContentPart
    {
        // Maximum length that MySql can support in an index under utf8 collation is 768,
        // minus 1 for the `DocumentId` integer (character size = integer size = 4 bytes),
        // minus 26 for the `ContentItemId` and 1 for the 'Published' and 'Latest' bools.
        public const int MaxAliasLength = 740;

        public string Alias { get; set; }
    }
}
