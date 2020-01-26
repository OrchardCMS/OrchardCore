using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class MultiValueFieldIndexHandler : ContentFieldIndexHandler<MultiValueField>
    {
        public override Task BuildIndexAsync(MultiValueField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();

            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key, string.Join(",", field.Values), options);
            }

            return Task.CompletedTask;
        }
    }
}
