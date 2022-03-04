using Atata;
using Atata.Bootstrap;

namespace OrchardCore.Tests.UI.Pages
{
    // Atata convention.
#pragma warning disable IDE0065 // Misplaced using directive
    using _ = OrchardCoreContentItemsPage;
#pragma warning restore IDE0065 // Misplaced using directive

    [Url("Admin/Contents/ContentItems")]
    public class OrchardCoreContentItemsPage : OrchardCoreAdminPage<_>
    {
        [FindById("new-dropdown")]
        public NewItemDropdown NewDropdown { get; private set; }

        public Link<_> NewPageLink { get; private set; }

        [FindById("items-form")]
        public UnorderedList<ContentListItem, _> Items { get; private set; }

        public OrchardCoreNewPageItemPage CreateNewPage() =>
            (NewPageLink.IsVisible ? NewPageLink : NewDropdown.Page)
                .ClickAndGo<OrchardCoreNewPageItemPage>();

        public sealed class NewItemDropdown : BSDropdownToggle<_>
        {
            public Link<_> Page { get; private set; }
        }

        [ControlDefinition("li[position() > 1]", ComponentTypeName = "item")]
        public sealed class ContentListItem : ListItem<_>
        {
            [FindByXPath("a")]
            public Text<_> Title { get; private set; }

            [FindByClass]
            public Link<_> View { get; private set; }
        }
    }
}
