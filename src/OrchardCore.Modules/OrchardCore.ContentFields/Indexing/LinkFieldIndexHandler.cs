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

            context.DocumentIndex.Set(context.Key, field.Url, options);
            context.DocumentIndex.Set(context.Key, field.Text, options);

            return Task.CompletedTask;
        }
    }
}
