using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentsTransfer.ViewModels;

public class ContentTransferEntryViewModel : ShapeViewModel
{
    public ContentTransferEntry ContentTransferEntry { get; set; }

    public ContentTransferEntryViewModel()
    {
    }

    public ContentTransferEntryViewModel(ContentTransferEntry entry)
    {
        ContentTransferEntry = entry;
    }
}
