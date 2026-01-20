using OrchardCore.ContentsTransfer.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentsTransfer.Drivers;

public sealed class ContentTransferEntryDisplayDriver : DisplayDriver<ContentTransferEntry>
{
    public override Task<IDisplayResult> DisplayAsync(ContentTransferEntry entry, BuildDisplayContext context)
    {
        return CombineAsync(
            View("ContentTransferEntriesMeta_SummaryAdmin", new ContentTransferEntryViewModel(entry)).Location("SummaryAdmin", "Meta:20"),
            View("ContentTransferEntriesActions_SummaryAdmin", new ContentTransferEntryViewModel(entry)).Location("SummaryAdmin", "Actions:5"),
            View("ContentTransferEntriesButtonActions_SummaryAdmin", new ContentTransferEntryViewModel(entry)).Location("SummaryAdmin", "ActionsMenu:10"),
            View("ContentTransferEntriesProgress_SummaryAdmin", new ContentTransferEntryViewModel(entry)).Location("SummaryAdmin", "Progress:5")
        );
    }
}
