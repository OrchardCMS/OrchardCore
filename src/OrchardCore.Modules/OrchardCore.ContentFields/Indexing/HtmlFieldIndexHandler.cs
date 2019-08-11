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

            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key, field.Html, options);
            }

            return Task.CompletedTask;
        }
    }
}
