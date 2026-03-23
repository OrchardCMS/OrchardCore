using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentTransfer.Models;

namespace OrchardCore.ContentTransfer.ViewModels;

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
