using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class ContentPickerFieldIndexHandler : ContentFieldIndexHandler<ContentPickerField>
    {
        public override Task BuildIndexAsync(ContentPickerField field, BuildFieldIndexContext context)
        {
            var options = DocumentIndexOptions.Keyword | DocumentIndexOptions.Store;

            if (field.ContentItemIds.Length > 0)
            {
                foreach (var contentItemId in field.ContentItemIds)
                {
                    foreach (var key in context.Keys)
                    {
                        context.DocumentIndex.Set(key, contentItemId, options);
                    }
                }
            }
            else
            {
                foreach (var key in context.Keys)
                {
                    context.DocumentIndex.Set(key, "NULL", options);
                }
            }

            return Task.CompletedTask;
        }
    }
}
