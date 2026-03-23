using OrchardCore.DisplayManagement;

namespace OrchardCore.ContentsTransfer.ViewModels;

public class BulkExportViewModel
{
    public ContentExporterViewModel Exporter { get; set; }

    public IShape List { get; set; }
}
