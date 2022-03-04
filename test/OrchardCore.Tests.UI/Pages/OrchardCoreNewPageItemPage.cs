using Atata;

namespace OrchardCore.Tests.UI.Pages
{
    // Atata convention.
#pragma warning disable IDE0065 // Misplaced using directive
    using _ = OrchardCoreNewPageItemPage;
#pragma warning restore IDE0065 // Misplaced using directive

    public class OrchardCoreNewPageItemPage : OrchardCoreAdminPage<_>
    {
        [FindByName("TitlePart.Title")]
        public TextInput<_> Title { get; private set; }

        [FindByName("submit.Publish")]
        public Button<OrchardCoreContentItemsPage, _> Publish { get; private set; }
    }
}
