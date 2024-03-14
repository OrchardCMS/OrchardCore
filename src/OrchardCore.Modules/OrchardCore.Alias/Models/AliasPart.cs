using OrchardCore.ContentManagement;

namespace OrchardCore.Alias.Models
{
    public class AliasPart : ContentPart
    {
        // Maximum length that MySql can support in an index under utf8mb4 collation is 768,
        // minus 2 for the `DocumentId` integer (bigint size = 8 bytes = 2 character size),
        // minus 26 for the `ContentItemId` and 1 for the 'Published' and 'Latest' bools.
        // minus 4 to allow at least to add a new integer column.
        public const int MaxAliasLength = 735;

        public string Alias { get; set; }
    }
}
