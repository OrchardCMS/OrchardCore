using System.Collections.Generic;
using OrchardCore.ContentsTransfer.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentsTransfer.Drivers;

public class ContentTransferEntryDisplayDriver : DisplayDriver<ContentTransferEntry>
{
    public override IDisplayResult Display(ContentTransferEntry entry)
    {
        var results = new List<IDisplayResult>()
        {
            Shape("ContentTransferEntriesMeta_SummaryAdmin", new ContentTransferEntryViewModel(entry)).Location("SummaryAdmin", "Meta:20"),
            Shape("ContentTransferEntriesActions_SummaryAdmin", new ContentTransferEntryViewModel(entry)).Location("SummaryAdmin", "Actions:5"),
            Shape("ContentTransferEntriesButtonActions_SummaryAdmin", new ContentTransferEntryViewModel(entry)).Location("SummaryAdmin", "ActionsMenu:10"),
            Shape("ContentTransferEntriesProgress_SummaryAdmin", new ContentTransferEntryViewModel(entry)).Location("SummaryAdmin", "Progress:5"),
        };

        return Combine(results);
    }
}
