using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentFields.Extentions;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class HtmlFieldIndexHandler : ContentFieldIndexHandler<HtmlField>
    {
        private const int MaxStringLength = 32766;
        private const int IndexOverlapLength = 100;

        public override Task BuildIndexAsync(HtmlField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();

            foreach (var key in context.Keys)
            {
                //context.DocumentIndex.Set(key, field.Html, options);
                foreach (var chunk in field.Html.Chunk(MaxStringLength, IndexOverlapLength))
                {
                    context.DocumentIndex.Set(key, new string(chunk), options);
                }
            }

            return Task.CompletedTask;
        }
    }
}
