using OrchardCore.Taxonomies.Fields;

namespace OrchardCore.Taxonomies.ViewModels
{
    public class DisplayTaxonomyFieldTagsViewModel : DisplayTaxonomyFieldViewModel
    {
        public string[] TagNames => Field.GetTagNames();
    }
}
