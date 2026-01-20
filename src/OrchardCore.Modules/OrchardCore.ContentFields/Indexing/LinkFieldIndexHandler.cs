using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class LinkFieldIndexHandler : ContentFieldIndexHandler<LinkField>
    {
        public override Task BuildIndexAsync(LinkField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();

            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key, field.Url, options);
                context.DocumentIndex.Set(key, field.Text, options);
            }

            return Task.CompletedTask;
        }
    }
}
