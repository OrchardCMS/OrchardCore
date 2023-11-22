using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentsTransfer.Models;

namespace OrchardCore.ContentsTransfer.ViewModels;

public class ListContentTransferEntriesViewModel
{
    public ListContentTransferEntryOptions Options { get; set; }

    [BindNever]
    public IList<dynamic> Entries { get; set; }

    [BindNever]
    public dynamic Header { get; set; }

    [BindNever]
    public dynamic Pager { get; set; }
}
