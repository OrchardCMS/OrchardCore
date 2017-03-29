using System.Threading.Tasks;
using Orchard.ContentFields.Fields;
using Orchard.Indexing;

namespace Orchard.ContentFields.Indexing
{
    public class HtmlFieldIndexHandler : ContentFieldIndexHandler<HtmlField>
    {
        public override Task BuildIndexAsync(HtmlField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();
            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(field.Html, DocumentIndex.Types.Text, options));

            return Task.CompletedTask;
        }
    }
}
