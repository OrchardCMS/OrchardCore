using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Handlers;
public interface IBulkContentHandler
{
    Task ImportingAsync(IEnumerable<ImportContentContext> contentItems);
    Task ImportedAsync(IEnumerable<ImportContentContext> contentItems);
}
