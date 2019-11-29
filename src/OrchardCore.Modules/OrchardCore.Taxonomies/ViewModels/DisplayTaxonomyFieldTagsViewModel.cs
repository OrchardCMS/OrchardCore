using OrchardCore.Taxonomies.Helper;

namespace OrchardCore.Taxonomies.ViewModels
{
    public class DisplayTaxonomyFieldTagsViewModel : DisplayTaxonomyFieldViewModel
    {
        public string[] Tags => Field.GetTags();
    }
}
