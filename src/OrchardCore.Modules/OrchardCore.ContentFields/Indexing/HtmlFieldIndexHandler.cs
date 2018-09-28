using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class HtmlFieldIndexHandler : ContentFieldIndexHandler<HtmlField>
    {
        public override Task BuildIndexAsync(HtmlField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();

            context.DocumentIndex.Set(context.Key, field.Html, options);

            return Task.CompletedTask;
        }
    }
}
