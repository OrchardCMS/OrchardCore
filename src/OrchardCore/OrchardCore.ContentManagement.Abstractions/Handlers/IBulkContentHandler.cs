using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Handlers;

public interface IBulkContentHandler
{
    Task ImportingAsync(IEnumerable<ImportContentContext> contexts);
    Task ImportedAsync(IEnumerable<ImportContentContext> contexts);
}
