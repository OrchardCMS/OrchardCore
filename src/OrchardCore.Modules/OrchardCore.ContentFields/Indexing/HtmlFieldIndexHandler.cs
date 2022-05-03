using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class HtmlFieldIndexHandler : ContentFieldIndexHandler<HtmlField>
    {
        private const int maxStringLength = 32766;

        public override Task BuildIndexAsync(HtmlField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();

            foreach (var key in context.Keys)
            {
                foreach (var chunk in field.Html.Chunk(MaxStringLength))
                {
                    context.DocumentIndex.Set(key, new string(chunk), options);
                )
            }

            return Task.CompletedTask;
        }
    }
}
