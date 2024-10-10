using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Handlers;

public interface IBulkContentEventHandler
{
    Task ImportingAsync(IEnumerable<ImportContentContext> contexts);
    Task ImportedAsync(IEnumerable<ImportContentContext> contexts);
}
