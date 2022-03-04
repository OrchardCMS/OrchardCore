using Atata;
using Atata.Bootstrap;
using Lombiq.Tests.UI.Components;

namespace OrchardCore.Tests.UI.Pages
{
    // Atata convention.
#pragma warning disable IDE0065 // Misplaced using directive
    using _ = OrchardCoreFeaturesPage;
#pragma warning restore IDE0065 // Misplaced using directive

    [Url("Admin/Features")]
    public sealed class OrchardCoreFeaturesPage : OrchardCoreAdminPage<_>
    {
        [FindById]
        public SearchInput<_> SearchBox { get; private set; }

        [FindById("bulk-action-menu-button")]
        public BulkActionsDropdown BulkActions { get; private set; }

        public FeatureItemList Features { get; private set; }

        public FeatureItem SearchForFeature(string featureName) =>
            SearchBox.Set(featureName)
                .Features[featureName];

        public sealed class BulkActionsDropdown : BSDropdownToggle<_>
        {
            public Link<_> Enable { get; private set; }

            public Link<_> Disable { get; private set; }

            public Link<_> Toggle { get; private set; }
        }

        [ControlDefinition("li[not(contains(@class, 'd-none'))]", ContainingClass = "list-group-item", ComponentTypeName = "feature")]
        public sealed class FeatureItem : Control<_>
        {
            [FindFirst(Visibility = Visibility.Any)]
            [ClicksUsingActions]
            public CheckBox<_> CheckBox { get; private set; }

            [FindByXPath("h6", "label")]
            public Text<_> Name { get; private set; }

            [FindById(TermMatch.StartsWith, "btn-enable")]
            public Link<_> Enable { get; private set; }

            [FindById(TermMatch.StartsWith, "btn-disable")]
            [GoTemporarily]
            public Link<ConfirmationModal<_>, _> Disable { get; private set; }

            public _ DisableWithConfirmation() =>
                Disable.ClickAndGo()
                    .Yes.ClickAndGo();

            protected override bool GetIsEnabled() => !Enable.IsVisible;
        }

        public sealed class FeatureItemList : ControlList<FeatureItem, _>
        {
            public FeatureItem this[string featureName] =>
                GetItem(featureName, item => item.Name == featureName);
        }
    }
}
