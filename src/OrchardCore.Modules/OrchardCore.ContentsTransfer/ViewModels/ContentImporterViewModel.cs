using System.Collections.Generic;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement;

namespace OrchardCore.ContentsTransfer.ViewModels;

public class ContentImporterViewModel
{
    public ContentTypeDefinition ContentTypeDefinition { get; set; }

    public IShape Content { get; set; }

    public IEnumerable<ImportColumn> Columns { get; set; }
}
