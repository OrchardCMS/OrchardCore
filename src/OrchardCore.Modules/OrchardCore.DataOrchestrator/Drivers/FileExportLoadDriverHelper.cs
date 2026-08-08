using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DataOrchestrator.Exporting;

namespace OrchardCore.DataOrchestrator.Drivers;

internal static class FileExportLoadDriverHelper
{
    public static IList<SelectListItem> BuildFormatOptions(IEtlExportFormatProvider formatProvider)
    {
        return formatProvider.ListFormats()
            .Select(format => new SelectListItem
            {
                Text = format.DisplayText,
                Value = format.Name,
            })
            .ToList();
    }
}
