using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Display;
using OrchardCore.DataOrchestrator.Exporting;
using OrchardCore.DataOrchestrator.ViewModels;

namespace OrchardCore.DataOrchestrator.Drivers;

public sealed class MediaFolderExportLoadDisplayDriver : EtlActivityDisplayDriver<MediaFolderExportLoad, MediaFolderExportLoadViewModel>
{
    private readonly IEtlExportFormatProvider _formatProvider;

    public MediaFolderExportLoadDisplayDriver(IEtlExportFormatProvider formatProvider)
    {
        _formatProvider = formatProvider;
    }

    protected override void EditActivity(MediaFolderExportLoad activity, MediaFolderExportLoadViewModel model)
    {
        model.Format = activity.Format;
        model.FileName = activity.FileName;
        model.Formats = FileExportLoadDriverHelper.BuildFormatOptions(_formatProvider);
    }

    protected override void UpdateActivity(MediaFolderExportLoadViewModel model, MediaFolderExportLoad activity)
    {
        activity.Format = model.Format;
        activity.FileName = model.FileName;
    }
}
